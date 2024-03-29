﻿using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableDouble : EditableConfigBase<double>
    {
        protected override double ParseValue(object value)
        {
            var minMax = MinMax();
            var min = (double)minMax.Min;
            var max = (double) minMax.Max;
            if (TryCastNumeric<double>(value, out var asDouble))
            {
                if (IsNumericValueAllowed(asDouble))
                    return asDouble;
                else if(IsValueAllowed(asDouble))
                {
                    if (asDouble > max)
                        return max;

                    if (asDouble < min)
                        return min;
                }
            }

            // given value is not allowed, return the current set value instead
            if (Value is double currentValue && IsNumericValueAllowed(currentValue))
                return currentValue;

            var predefinedValues = PredefinedValues;
            if (predefinedValues.Count == 0)
            {
                var newValue = min;
                while (!IsNumericValueAllowed(newValue) && newValue <= max)
                {
                    newValue += 1;
                }

                return newValue;
            }

            return PredefinedValues.OfType<double>().FirstOrDefault();
        }

        public EditableDouble(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory, ChangeTrackingManager changeTrackingManager) : base(forInstance, propertyInfo, factory, changeTrackingManager)
        {
            // parse the existing value on the instance
            Value = Value;
        }
    }
}