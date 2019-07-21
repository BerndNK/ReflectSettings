using System.Collections.Generic;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableCollection<TItem> : EditableConfigBase<ICollection<TItem>>
    {
        public EditableCollection(object forInstance, PropertyInfo propertyInfo, EditableConfigFactory factory) : base(forInstance, propertyInfo, factory)
        {
            Value = Value;
        }

        protected override ICollection<TItem> ParseValue(object value)
        {
            return value as ICollection<TItem>;
        }
    }
}
