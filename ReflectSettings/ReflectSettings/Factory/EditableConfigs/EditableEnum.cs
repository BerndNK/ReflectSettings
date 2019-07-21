using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.Factory.EditableConfigs
{
    internal class EditableEnum<T> : EditableConfigBase<T> where T : struct
    {
        public EditableEnum(object forInstance, PropertyInfo propertyInfo) : base(forInstance, propertyInfo)
        {
        }

        protected override T ParseValue(object value)
        {
            var asString = value as string ?? value?.ToString();

            if (Enum.TryParse<T>(asString, out var asEnum))
            {
                if (IsEnumValueAllowed(asEnum))
                    return asEnum;
            }

            // given value is not allowed, return the current set value instead
            if (Value is T currentValue && IsEnumValueAllowed(currentValue))
                return currentValue;

            // current value is not the enum type, try to use the first PredefinedValue or default
            var allowedValues = PredefinedValues().ToList();
            if (allowedValues.Count == 0)
                return PossibleEnumValues().FirstOrDefault(IsEnumValueAllowed);
            else
                return allowedValues.FirstOrDefault();
        }

        private bool IsEnumValueAllowed(T value)
        {
            if (!IsValueAllowed(value))
                return false;

            var possibleEnumValues = PossibleEnumValues();
            if (possibleEnumValues.Contains(value))
                return true;

            return false;
        }

        private IEnumerable<T> PossibleEnumValues()
        {
            var type = typeof(T);
            return type.IsEnum
                ? Enum.GetValues(type).OfType<T>()
                : Enumerable.Empty<T>();
        }
    }
}