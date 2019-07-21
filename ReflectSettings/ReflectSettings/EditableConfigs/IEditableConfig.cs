using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public interface IEditableConfig
    {
        PropertyInfo PropertyInfo { get; set; }

        object Value { get; set; }
    }
}