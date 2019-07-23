using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ReflectSettings.Annotations;
using ReflectSettings.Attributes;

namespace ReflectSettings.EditableConfigs
{
    public abstract class EditableConfigBase<T> : IEditableConfig
    {
        private readonly IList<Attribute> _attributes;
        private ChangeTrackingManager _changeTrackingManager;

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
                SetValue(newValue);
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
            UpdateCalculatedValues();
        }

        private TAttribute Attribute<TAttribute>() where TAttribute : Attribute =>
            _attributes.OfType<TAttribute>().FirstOrDefault() ?? Activator.CreateInstance<TAttribute>();

        protected MinMaxAttribute MinMax() => Attribute<MinMaxAttribute>();

        public ObservableCollection<object> PredefinedValues { get; } = new ObservableCollection<object>();

        public bool HasPredefinedValues => _attributes.OfType<PredefinedValuesAttribute>().FirstOrDefault() != null ||
                                           _attributes.OfType<CalculatedValuesAttribute>().FirstOrDefault() != null ||
                                           PropertyInfo.PropertyType.IsEnum;

        public ChangeTrackingManager ChangeTrackingManager
        {
            get => _changeTrackingManager;
            set
            {
                _changeTrackingManager?.Remove(this);

                _changeTrackingManager = value;

                _changeTrackingManager?.Add(this);
            }
        }

        public void UpdateCalculatedValues()
        {
            if (!HasPredefinedValues)
                return;

            var existingValues = PredefinedValues.ToList();
            var newValues = GetPredefinedValues().OfType<object>().ToList();

            var toRemove = existingValues.Except(newValues);
            var toAdd = newValues.Except(existingValues);

            var somethingChanged = false;
            foreach (var value in toAdd)
            {
                PredefinedValues.Add(value);
                somethingChanged = true;
            }

            foreach (var value in toRemove)
            {
                PredefinedValues.Remove(value);
                somethingChanged = true;
            }

            if (somethingChanged)
                Value = Value;
        }

        protected IEnumerable<T> GetPredefinedValues()
        {
            var staticValues = Attribute<PredefinedValuesAttribute>();
            var calculatedValuesAttribute = Attribute<CalculatedValuesAttribute>();

            var calculatedValues = calculatedValuesAttribute.CallMethod(ForInstance);

            var concat = staticValues.Values.Concat(calculatedValues).ToList();
            var toReturn = concat.OfType<T>().Except(ForbiddenValues()).ToList();

            if (concat.Any(x => x == null))
            {
                toReturn.Add(default);
            }

            return toReturn;
        }

        protected TObject InstantiateObject<TObject>()
        {
            var targetType = typeof(TObject);
            var typeToInstantiate = targetType;

            if (targetType.IsInterface)
                typeToInstantiate = PossibleTypesFor(targetType).First();

            return (TObject) Activator.CreateInstance(typeToInstantiate);
        }

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
            if (value == null)
                return false;

            var predefinedValues = GetPredefinedValues().ToList();
            var isValueAllowed = predefinedValues.Count == 0 || predefinedValues.Any(v => v.Equals(value));
            var isValueForbidden = ForbiddenValues().Any(v => v.Equals(value));

            return isValueAllowed && !isValueForbidden;
        }

        protected bool IsNumericValueAllowed(dynamic numericValue)
        {
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
                    result = (TNumeric) castMethod(value is string asString ? asString.Replace(',','.') : value);
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}