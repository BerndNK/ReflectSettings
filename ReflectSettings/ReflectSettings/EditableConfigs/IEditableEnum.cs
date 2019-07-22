using System.Collections.Generic;

namespace ReflectSettings.EditableConfigs
{
    public interface IEditableEnum : IEditableConfig
    {
        IEnumerable<object> EnumValues { get; }
    }
}