using System.Reflection;

namespace ReflectSettings.Factory.EditableConfigs
{
    public class EditableDummy : EditableConfigBase<object>
    {
        public EditableDummy(object forInstance, PropertyInfo propertyInfo) : base(forInstance, propertyInfo)
        {
        }

        protected override object ParseValue(object value) => value;
    }
}