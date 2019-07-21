using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableString : EditableConfigBase<string>
    {
        protected override string ParseValue(object value)
        {
            var asString = value as string ?? value?.ToString();

            if (IsValueAllowed(asString))
                return asString;
            
            // given value is not allowed, return the current set value instead
            var currentValue = Value as string ?? Value?.ToString();

            // if current value is also null, try to use the first PredefinedValue 
            var allowedValues = PredefinedValues().ToList();
            return currentValue ?? allowedValues.FirstOrDefault();
        }

        public EditableString(object forInstance, PropertyInfo propertyInfo, EditableConfigFactory factory) : base(forInstance, propertyInfo, factory)
        {
            // parse the existing value on the instance
            Value = Value;
        }
    }
}