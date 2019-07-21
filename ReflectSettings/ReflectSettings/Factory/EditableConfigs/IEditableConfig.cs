using System.Reflection;

namespace ReflectSettings.Factory.EditableConfigs
{
    public interface IEditableConfig
    {
        PropertyInfo PropertyInfo { get; set; }

        object Value { get; set; }
    }
}