using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableKeyValuePair<TKey, TValue> : EditableConfigBase<KeyValuePair<TKey, TValue>>,
        IEditableKeyValuePair
    {
        public EditableKeyValuePair(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory) : base(
            forInstance, propertyInfo, factory)
        {
        }

        protected override KeyValuePair<TKey, TValue> ParseValue(object value)
        {
            if (value is KeyValuePair<TKey, TValue> asKeyValuePair && IsValueAllowed(asKeyValuePair))
                return asKeyValuePair;

            // given value is not allowed, return the current set value instead
            if (Value is KeyValuePair<TKey, TValue> currentValue && IsValueAllowed(currentValue))
                return currentValue;

            var predefinedValues = GetPredefinedValues().ToList();
            if (predefinedValues.Count == 0)
            {
                return new KeyValuePair<TKey, TValue>();
            }

            return GetPredefinedValues().FirstOrDefault();
        }

        private KeyValuePair<TKey, TValue> AsKeyValuePair => Value is KeyValuePair<TKey, TValue> asKeyValuePair
            ? asKeyValuePair
            : new KeyValuePair<TKey, TValue>();

        public object Key
        {
            get => AsKeyValuePair.Key;
            set
            {
                if (!(value is TKey asKey))
                    return;

                var newPair = new KeyValuePair<TKey, TValue>(asKey, (TValue) PairValue);
                Value = newPair;
            }
        }

        public object PairValue
        {
            get => AsKeyValuePair.Value;
            set
            {
                if (!(value is TValue asValue))
                    return;

                var newPair = new KeyValuePair<TKey, TValue>((TKey) Key, asValue);
                Value = newPair;
            }
        }
    }
}