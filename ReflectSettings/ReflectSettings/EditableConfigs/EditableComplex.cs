using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableComplex<T> : EditableConfigBase<T>, IEditableComplex where T : class
    {
        private object _additionalData;

        protected override T ParseValue(object value)
        {
            if (value is T asT)
            {
                if (IsValueAllowed(asT))
                {
                    CreateSubEditables(asT);
                    return asT;
                }
                else if (Value is T currentValue && IsValueAllowed(currentValue))
                {
                    return currentValue;
                }
            }

            // if null is allowed, return null
            var isNullAllowed = PredefinedValues.Any(x => x == null);
            if (isNullAllowed)
                return null;
            
            var predefinedValuesOfT = PredefinedValues.OfType<T>().ToList();
            if (predefinedValuesOfT.Any())
                return predefinedValuesOfT.First();

            // otherwise create a new instance
            var newInstance = (T) InstantiateObjectOfSpecificType(GetPredefinedType());
            CreateSubEditables(newInstance);
            return newInstance;
        }

        public new object AdditionalData
        {
            get => _additionalData;
            set
            {
                _additionalData = value;
                foreach (var config in SubEditables)
                {
                    config.AdditionalData = value;
                }

                OnPropertyChanged();
            }
        }

        protected virtual void CreateSubEditables(T fromInstance)
        {
            SubEditables.Clear();
            if (fromInstance != null)
            {
                var subEditables = Factory.Reflect(fromInstance, ChangeTrackingManager).ToList();
                foreach (var item in subEditables)
                {
                    item.CalculatedValues.InheritFrom(CalculatedValues);
                    item.CalculatedValuesAsync.InheritFrom(CalculatedValuesAsync);
                    item.CalculatedTypes.InheritFrom(CalculatedTypes);
                    item.CalculatedVisibility.InheritFrom(CalculatedVisibility);
                    if (item.IsDisplayNameProperty)
                        item.PropertyChanged += OnDisplayNameEditablePropertyChanged;
                    item.UpdateCalculatedValues();
                    item.AdditionalData = AdditionalData;
                    SubEditables.Add(item);
                }
            }
        }

        private void OnDisplayNameEditablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(DisplayName));
        }

        public ObservableCollection<IEditableConfig> SubEditables { get; private set; } =
            new ObservableCollection<IEditableConfig>();

        protected override string ResolveDisplayName()
        {
            var subEditableThatIsDisplayName = SubEditables.FirstOrDefault(x => x.IsDisplayNameProperty);
            var displayName = subEditableThatIsDisplayName?.Value?.ToString();
            if (displayName != null)
                return displayName;

            displayName = Value?.ToString();
            if (displayName != null)
                return displayName;

            return base.ResolveDisplayName();
        }

        public EditableComplex(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory, ChangeTrackingManager changeTrackingManager) : base(
            forInstance, propertyInfo, factory, changeTrackingManager)
        {
            if (propertyInfo.PropertyType != typeof(T))
                throw new ArgumentException(
                    $"Given property was not type of T of this instance. Property type: {propertyInfo.PropertyType} T: {typeof(T)}");

            // parse the existing value on the instance
            Value = Value;
        }


        public override void UpdateCalculatedValues()
        {
            base.UpdateCalculatedValues();
            foreach (var editable in SubEditables)
            {
                editable.CalculatedValues.InheritFrom(CalculatedValues);
                editable.CalculatedValuesAsync.InheritFrom(CalculatedValuesAsync);
                editable.CalculatedTypes.InheritFrom(CalculatedTypes);
                editable.CalculatedVisibility.InheritFrom(CalculatedVisibility);
            }
        }
    }
}