using System.Collections.ObjectModel;

namespace ReflectSettings.EditableConfigs
{
    public interface IEditableComplex : IEditableConfig
    {
        ObservableCollection<IEditableConfig> SubEditables { get; }
    }
}