using System.Reflection;
using System.Security;

namespace ReflectSettings.EditableConfigs
{
    public class EditableSecureString : EditableConfigBase<SecureString>
    {
        protected override SecureString ParseValue(object value)
        {
            if (value is SecureString asASecureString)
                return asASecureString;

            return new SecureString();
        }

        public EditableSecureString(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory, ChangeTrackingManager changeTrackingManager) : base(forInstance, propertyInfo, factory, changeTrackingManager)
        {
        }
    }
}
