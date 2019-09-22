using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ReflectSettings.Annotations;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs.InheritingAttribute;

namespace ReflectSettings.EditableConfigs
{
    public abstract class EditableConfigBase<T> : IEditableConfig
    {
        private readonly IList<Attribute> _attributes;
        private ChangeTrackingManager _changeTrackingManager;
        private object _additionalData;

        protected SettingsFactory Factory { get; }

        public object ForInstance { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public object Value
        {
            get => GetValue();
            set
            {
                var newValue = ParseValue(value);
                if (Equals(newValue, Value))
                    return;
                var oldValue = Value;
                SetValue(newValue);
                ValueChanged?.Invoke(this, new EditableConfigValueChangedEventArgs(oldValue, Value));
                OnPropertyChanged();
            }
        }

        protected abstract T ParseValue(object value);

        private T GetValue() => (T) PropertyInfo.GetValue(ForInstance);

        private void SetValue(T value) => PropertyInfo.SetValue(ForInstance, value);

        protected EditableConfigBase(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory)
        {
            ForInstance = forInstance;
            PropertyInfo = propertyInfo;
            Factory = factory;

            _attributes = propertyInfo.GetCustomAttributes(true).OfType<Attribute>().ToList();
            InitCalculatedAttributes();
            UpdateCalculatedValues();
        }

        private TAttribute Attribute<TAttribute>() where TAttribute : Attribute =>
            _attributes.OfType<TAttribute>().FirstOrDefault() ?? Activator.CreateInstance<TAttribute>();

        private IEnumerable<TAttribute> Attributes<TAttribute>() where TAttribute : Attribute =>
            _attributes.OfType<TAttribute>();

        protected MinMaxAttribute MinMax() => Attribute<MinMaxAttribute>();

        public ObservableCollection<object> PredefinedValues { get; } = new ObservableCollection<object>();

        public bool HasPredefinedValues => _attributes.OfType<PredefinedValuesAttribute>().Any() ||
                                           CalculatedValues.ForThis.Any() ||
                                           CalculatedValuesAsync.ForThis.Any() ||
                                           PropertyInfo.PropertyType.IsEnum;

        public bool HasCalculatedType => CalculatedTypes.ForThis.Any();

        public ChangeTrackingManager ChangeTrackingManager
        {
            get => _changeTrackingManager;
            set
            {
                _changeTrackingManager?.Remove(this);

                _changeTrackingManager = value;

                _changeTrackingManager?.Add(this);

                SetChangeTrackingManagerForChildren(value);
            }
        }

        protected virtual void SetChangeTrackingManagerForChildren(ChangeTrackingManager value)
        {
        }

        public bool IsDisplayNameProperty => _attributes.OfType<IsDisplayName>().FirstOrDefault() != null;

        public virtual void UpdateCalculatedValues()
        {
            OnPropertyChanged(nameof(IsHidden));
            if (!HasPredefinedValues && !HasCalculatedType)
                return;

            if (CalculatedValuesAsync.ForThis.Any())
            {
                if (_currentlyRunningCalculatingValuesTask != null &&
                    _currentlyRunningCalculatingValuesTask.Status != TaskStatus.RanToCompletion)
                {
                    _taskThatCameInWhileAnotherWasStillRunning = UpdateCalculatedValuesAsync;
                    return;
                }

                var task = UpdateCalculatedValuesAsync();
                lock (_currentRunningThreadMutex)
                {
                    if (_currentlyRunningCalculatingValuesTask != null)
                    {
                        _taskThatCameInWhileAnotherWasStillRunning = UpdateCalculatedValuesAsync;
                    }
                    else
                    {
                        if (task.Status != TaskStatus.RanToCompletion)
                            _currentlyRunningCalculatingValuesTask = task;
                    }
                }

                return;
            }

            var existingValues = PredefinedValues.ToList();
            var newValuesOfT = GetPredefinedValues().ToList();
            var newValues = newValuesOfT.OfType<object>().ToList();
            if (newValuesOfT.Any(x => x == null))
                newValues.Add(null);

            var toRemove = existingValues.Except(newValues);
            var toAdd = newValues.Except(existingValues);

            var somethingChanged = false;
            foreach (var value in toAdd)
            {
                PredefinedValues.Add(value);
                somethingChanged = true;
            }

            if (somethingChanged)
            {
                Value = Value;
                somethingChanged = false;
            }

            foreach (var value in toRemove)
            {
                PredefinedValues.Remove(value);
                somethingChanged = true;
            }

            var valueTypeDiffersFromPredefined = Value?.GetType() != GetPredefinedType();


            if (somethingChanged || (HasCalculatedType && valueTypeDiffersFromPredefined))
            {
                Value = Value;
            }
        }

        private Task _currentlyRunningCalculatingValuesTask = null;
        private Mutex _currentRunningThreadMutex = new Mutex();
        private Func<Task> _taskThatCameInWhileAnotherWasStillRunning = null;
        private bool _loadingValuesAsync;

        public async Task UpdateCalculatedValuesAsync()
        {
            IsBusy = true;

            var existingValues = PredefinedValues.ToList();
            var newValues = GetPredefinedValues().OfType<object>().ToList();

            _loadingValuesAsync = true;
            foreach (var asyncValues in CalculatedValuesAsync.ForThis)
            {
                var result = await asyncValues.ResolveValuesAsync(CalculatedValuesAsync.Inherited);
                newValues = newValues.Concat(result).ToList();
            }

            _loadingValuesAsync = false;

            var toRemove = existingValues.Except(newValues);
            var toAdd = newValues.Except(existingValues);

            var somethingChanged = false;
            foreach (var value in toAdd)
            {
                PredefinedValues.Add(value);
                somethingChanged = true;
            }

            if (somethingChanged)
            {
                Value = Value;
                somethingChanged = false;
            }

            foreach (var value in toRemove)
            {
                PredefinedValues.Remove(value);
                somethingChanged = true;
            }

            var valueTypeDiffersFromPredefined = Value?.GetType() != GetPredefinedType();


            if (somethingChanged || (HasCalculatedType && valueTypeDiffersFromPredefined))
            {
                Value = Value;
            }

            if (_taskThatCameInWhileAnotherWasStillRunning != null)
            {
                
                var taskMethod = _taskThatCameInWhileAnotherWasStillRunning;
                _taskThatCameInWhileAnotherWasStillRunning = null;

                await taskMethod();
                Value = Value;
            }

            lock (_currentRunningThreadMutex)
            {
                _currentlyRunningCalculatingValuesTask = null;
            }
            
            IsBusy = false;
        }


        private void InitCalculatedAttributes()
        {
            foreach (var attribute in _attributes.OfType<INeedsInstance>())
            {
                attribute.AttachedToInstance = ForInstance;
            }

            CalculatedVisibility =
                new InheritedAttributes<CalculatedVisibilityAttribute>(_attributes.OfType<CalculatedVisibilityAttribute>());
            CalculatedValues =
                new InheritedAttributes<CalculatedValuesAttribute>(_attributes.OfType<CalculatedValuesAttribute>());
            CalculatedTypes =
                new InheritedAttributes<CalculatedTypeAttribute>(_attributes.OfType<CalculatedTypeAttribute>());
            CalculatedValuesAsync =
                new InheritedAttributes<CalculatedValuesAsyncAttribute>(_attributes
                    .OfType<CalculatedValuesAsyncAttribute>());
        }

        public InheritedAttributes<CalculatedVisibilityAttribute> CalculatedVisibility { get; private set; }

        public InheritedAttributes<CalculatedTypeAttribute> CalculatedTypes { get; private set; }

        public InheritedAttributes<CalculatedValuesAttribute> CalculatedValues { get; private set; }

        public InheritedAttributes<CalculatedValuesAsyncAttribute> CalculatedValuesAsync { get; private set; }

        private IEnumerable<T> GetPredefinedValues()
        {
            var staticValues = Attribute<PredefinedValuesAttribute>();
            // methods with a key are only used when the specific key is used as the resolution name of the attribute
            var calculatedValuesAttributes = CalculatedValues.ForThis;

            var calculatedValues =
                calculatedValuesAttributes.SelectMany(x => x.ResolveValues(CalculatedValues.Inherited));

            var concat = staticValues.Values.Concat(calculatedValues).ToList();
            var toReturn = concat.OfType<T>().Except(ForbiddenValues()).ToList();

            if (concat.Any(x => x == null))
            {
                toReturn.Add(default);
            }

            return toReturn;
        }

        protected Type GetPredefinedType()
        {
            // methods with a key are only used when the specific key is used as the resolution name of the attribute
            var calculatedTypeAttributes = CalculatedTypes.ForThis;

            var calculatedTypes =
                calculatedTypeAttributes.Select(x => x.CallMethod(CalculatedTypes.Inherited, ForInstance));

            return calculatedTypes.FirstOrDefault(IsAssignableToT) ?? typeof(T);
        }

        private bool IsAssignableToT(Type type) => typeof(T).IsAssignableFrom(type) && type != typeof(object);

        protected object InstantiateObjectOfSpecificType(Type type)
        {
            var method = GetType().GetMethod(nameof(InstantiateObject));
            if (method == null)
                return default;

            var asGeneric = method.MakeGenericMethod(type);

            return asGeneric.Invoke(this, null);
        }

        public TObject InstantiateObject<TObject>()
        {
            var targetType = typeof(TObject);
            var typeToInstantiate = targetType;

            if (targetType == typeof(string) && "" is TObject stringAsTObject)
                return stringAsTObject;

            if (targetType.IsPrimitive)
                return default;

            if (targetType.IsInterface)
                typeToInstantiate = PossibleTypesFor(targetType).First();

            if (!HasConstructorWithNoParameter(typeToInstantiate))
                return default;

            return (TObject) Activator.CreateInstance(typeToInstantiate);
        }

        private bool HasConstructorWithNoParameter(Type type) => type.GetConstructor(Type.EmptyTypes) != null;

        protected IEnumerable<Type> PossibleTypesFor(Type interfaceType)
        {
            var typesForInstantiation = Attribute<TypesForInstantiationAttribute>().Types;
            return typesForInstantiation.Where(interfaceType.IsAssignableFrom);
        }

        protected IEnumerable<T> ForbiddenValues()
        {
            var forbiddenValues = Attribute<ForbiddenValuesAttribute>().ForbiddenValues;

            return forbiddenValues.OfType<T>();
        }

        protected bool IsValueAllowed(T value)
        {
            // while possible values are loaded, allow anything (so the set value doesn't get lost due to this method saying no
            if (_loadingValuesAsync)
                return true;

            if (value == null)
                return false;

            var isTypeAllowed = GetPredefinedType().IsInstanceOfType(value);

            var predefinedValues = PredefinedValues;
            var isPredefinedValueOrNoPredefinedValuesGiven =
                predefinedValues.Count == 0 || predefinedValues.Any(v => v.Equals(value));

            var isValueAllowed = isPredefinedValueOrNoPredefinedValuesGiven && isTypeAllowed;
            var isValueForbidden = ForbiddenValues().Any(v => v.Equals(value));


            return isValueAllowed && !isValueForbidden;
        }

        protected bool IsNumericValueAllowed(dynamic numericValue)
        {
            // while possible values are loaded, allow anything (so the set value doesn't get lost due to this method saying no
            if (_loadingValuesAsync)
                return true;

            var minMax = MinMax();

            if (!(numericValue is T asT))
                return false;

            if (!IsValueAllowed(asT))
                return false;

            return numericValue >= minMax.Min && numericValue <= minMax.Max;
        }

        protected bool TryCastNumeric<TNumeric>(object value, out TNumeric result)
        {
            try
            {
                var castMethod = CastMethod<TNumeric>();
                if (castMethod != null)
                    result = (TNumeric) castMethod(value is string asString ? asString.Replace(',', '.') : value);
                else
                    result = (TNumeric) value;
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        private Func<object, object> CastMethod<TNumeric>()
        {
            var type = typeof(TNumeric);

            if (type == typeof(double))
                return x => Convert.ToDouble(x, CultureInfo.InvariantCulture);

            if (type == typeof(int))
                return x => Convert.ToInt32(x, CultureInfo.InvariantCulture);

            if (type == typeof(float))
                return x => (float) Convert.ToDouble(x, CultureInfo.InvariantCulture);

            return x => x;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EditableConfigValueChangedEventArgs> ValueChanged;

        public string DisplayName => ResolveDisplayName();

        public bool IsHidden => _attributes.OfType<IsHiddenAttribute>().Any() || CalculatedVisibility.ForThis.Any(x => x.IsHidden(CalculatedVisibility.Inherited));

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public object AdditionalData
        {
            get => _additionalData;
            set
            {
                _additionalData = value;
                OnPropertyChanged();
            }
        }

        protected virtual string ResolveDisplayName()
        {
            return _attributes.OfType<NameAttribute>().FirstOrDefault()?.Name ?? PropertyInfo.Name;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsRemoving { get; set; }
    }
}