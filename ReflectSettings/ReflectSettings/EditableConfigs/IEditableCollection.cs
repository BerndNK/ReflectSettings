using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ReflectSettings.EditableConfigs
{
    public interface IEditableCollection : IEditableConfig
    {
        ObservableCollection<IEditableConfig> SubEditables { get; }

        ICommand AddNewItemCommand { get; }
    }
}