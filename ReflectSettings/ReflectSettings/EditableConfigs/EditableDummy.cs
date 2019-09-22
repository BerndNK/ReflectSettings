using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableDummy : EditableConfigBase<object>
    {
        public EditableDummy(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory, ChangeTrackingManager changeTrackingManager) : base(forInstance, propertyInfo, factory, changeTrackingManager)
        {
        }
        
        protected override object ParseValue(object value) => value;
    }
}