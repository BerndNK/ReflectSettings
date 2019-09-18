using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableBool : EditableConfigBase<bool>
    {
        public EditableBool(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory) : base(forInstance,
            propertyInfo, factory)
        {
            Value = Value;
        }

        protected override bool ParseValue(object value)
        {
            if (TryCastBool(value, out var asBool))
            {
                if (IsValueAllowed(asBool))
                    return asBool;
            }

            // given value is not allowed, return the current set value instead
            if (Value is bool currentValue && IsValueAllowed(currentValue))
                return currentValue;

            var predefinedValues = PredefinedValues.ToList();
            if (predefinedValues.Count == 0)
            {
                return !IsValueAllowed(false);
            }

            return PredefinedValues.OfType<bool>().FirstOrDefault();
        }

        private bool TryCastBool(object value, out bool result)
        {
            if (value is bool asBool)
            {
                result = asBool;
                return true;
            }

            var asString = value as string ?? value?.ToString();
            if (asString != null)
            {
                if (bool.TryParse(asString, out asBool))
                {
                    result = asBool;
                    return true;
                }
            }

            if(TryCastNumeric<dynamic>(value, out var asNumeric))
            {
                result = asNumeric == 1;
                return true;
            }

            result = false;
            return false;
        }
    }
}