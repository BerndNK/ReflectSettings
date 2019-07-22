using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public interface IEditableConfig : INotifyPropertyChanged
    {
        PropertyInfo PropertyInfo { get; set; }

        object Value { get; set; }

        ObservableCollection<object> PredefinedValues { get; }

        bool HasPredefinedValues { get; }

        ChangeTrackingManager ChangeTrackingManager { get; set; }

        void UpdateCalculatedValues();
    }
}