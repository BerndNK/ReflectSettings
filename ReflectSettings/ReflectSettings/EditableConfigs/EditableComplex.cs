using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableComplex<T> : EditableConfigBase<T> where T : class
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
            if (PredefinedValues().Any(x => x == null))
                return null;

            // otherwise create a new instance
            var newInstance = InstantiateObject();
            CreateSubEditables(newInstance);
            return newInstance;
        }

        private void CreateSubEditables(T fromInstance)
        {
            if (fromInstance == null)
                SubEditables = new List<IEditableConfig>();
            else
                SubEditables = Factory.Produce(fromInstance).ToList();
        }

        public IList<IEditableConfig> SubEditables { get; private set; } = new List<IEditableConfig>();

        public EditableComplex(object forInstance, PropertyInfo propertyInfo, EditableConfigFactory factory) : base(forInstance, propertyInfo, factory)
        {
            var hasConstructorWithNoParameter = HasConstructorWithNoParameter();
            if (!hasConstructorWithNoParameter)
                throw new ArgumentException(
                    $"Cannot create editable for type without parameter-less constructor. Type is: {propertyInfo.PropertyType}");

            if (propertyInfo.PropertyType != typeof(T))
                throw new ArgumentException(
                    $"Given property was not type of T of this instance. Property type: {propertyInfo.PropertyType} T: {typeof(T)}");

            // parse the existing value on the instance
            Value = Value;
        }

        private T InstantiateObject()
        {
            return Activator.CreateInstance<T>();
        }

        private bool HasConstructorWithNoParameter()
        {
            var constructors = typeof(T).GetConstructors(BindingFlags.Public).ToList();
            return constructors.Count == 0 || constructors.Any(c => c.GetParameters().Length == 0);
        }
    }
}