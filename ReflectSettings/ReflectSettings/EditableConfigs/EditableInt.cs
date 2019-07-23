using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableInt : EditableConfigBase<int>
    {
        protected override int ParseValue(object value)
        {
            var minMax = MinMax();
            var min = (int)minMax.Min;
            var max = (int) minMax.Max;
            if (TryCastNumeric<int>(value, out var asInt))
            {
                if (IsNumericValueAllowed(asInt))
                    return asInt;
                else if(IsValueAllowed(asInt))
                {
                    if (asInt > max)
                        return max;

                    if (asInt < min)
                        return min;
                }
            }
            else
            {
                var asString = value as string ?? value?.ToString();
                if (int.TryParse(asString, out asInt))
                {
                    if (IsNumericValueAllowed(asInt))
                        return asInt;
                }
            }

            // given value is not allowed, return the current set value instead
            if (Value is int currentValue && IsNumericValueAllowed(currentValue))
                return currentValue;

            var predefinedValues = GetPredefinedValues().ToList();
            if (predefinedValues.Count == 0)
            {
                var newValue = min;
                while (!IsNumericValueAllowed(newValue) && newValue <= max)
                {
                    newValue += 1;
                }

                return newValue;
            }

            return GetPredefinedValues().FirstOrDefault();
        }

        public EditableInt(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory) : base(forInstance, propertyInfo, factory)
        {
            // parse the existing value on the instance
            Value = Value;
        }
    }
}