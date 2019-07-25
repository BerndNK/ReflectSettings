using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class ReadonlyEditableCollection<TItem, TCollection> : EditableConfigBase<TCollection>, IReadOnlyEditableCollection,
        IReadOnlyCollection<TItem> where TCollection : class, IReadOnlyCollection<TItem>
    {
        public ReadonlyEditableCollection(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory) : base(forInstance, propertyInfo, factory)
        {
            Value = Value;
        }
        
        private IReadOnlyCollection<TItem> AsCollection => Value as IReadOnlyCollection<TItem> ?? new List<TItem>();

        protected override TCollection ParseValue(object value)
        {
            if (value is TCollection asT)
            {
                if (IsValueAllowed(asT))
                    return asT;
                else if (Value is TCollection currentValue && IsValueAllowed(currentValue))
                    return currentValue;
            }

            // if null is allowed, return null
            if (GetPredefinedValues().Any(x => x == null))
                return null;

            // otherwise create a new instance
            var newInstance = InstantiateObject<TCollection>();
            return newInstance;
        }

        public IEnumerator<TItem> GetEnumerator() =>  AsCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => AsCollection.Count;
    }
}
