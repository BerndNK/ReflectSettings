using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableComplex<T> : EditableConfigBase<T>, IEditableComplex where T : class
    {
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
            var predefinedValues = GetPredefinedValues().ToList();
            if (predefinedValues.Any(x => x == null))
                return null;

            if (predefinedValues.Any())
                return predefinedValues.First();

            // otherwise create a new instance
            var newInstance = InstantiateObject<T>();
            CreateSubEditables(newInstance);
            return newInstance;
        }

        private void CreateSubEditables(T fromInstance)
        {
            SubEditables.Clear();
            if (fromInstance != null)
            {
                var subEditables = Factory.Reflect(fromInstance).ToList();
                foreach (var item in subEditables)
                {
                    item.InheritedCalculatedValuesAttribute.AddRange(AllCalculatedValuesAttribute);
                    item.ChangeTrackingManager = ChangeTrackingManager;
                    item.Value = item.Value;
                    SubEditables.Add(item);
                }
            }
                
        }

        public ObservableCollection<IEditableConfig> SubEditables { get; private set; } = new ObservableCollection<IEditableConfig>();

        public EditableComplex(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory) : base(forInstance, propertyInfo, factory)
        {
            if (propertyInfo.PropertyType != typeof(T))
                throw new ArgumentException(
                    $"Given property was not type of T of this instance. Property type: {propertyInfo.PropertyType} T: {typeof(T)}");

            // parse the existing value on the instance
            Value = Value;
        }
    }
}