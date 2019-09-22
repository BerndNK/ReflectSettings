using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    internal class EditableEnum<T> : EditableConfigBase<T>, IEditableEnum where T : struct
    {
        public EditableEnum(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory, ChangeTrackingManager changeTrackingManager) : base(forInstance, propertyInfo, factory, changeTrackingManager)
        {
            // parse the existing value on the instance
            Value = Value;
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
            var allowedValues = PredefinedValues;
            if (allowedValues.Count == 0)
                return PossibleEnumValues().FirstOrDefault(IsEnumValueAllowed);
            else
                return allowedValues.OfType<T>().FirstOrDefault();
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

        public IEnumerable<object> EnumValues => PossibleEnumValues().Cast<object>();
    }
}