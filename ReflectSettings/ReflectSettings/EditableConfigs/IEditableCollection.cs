using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ReflectSettings.EditableConfigs
{
    public interface IEditableCollection : IEditableConfig, IEnumerable
    {
        ObservableCollection<IEditableConfig> SubEditables { get; }

        ICommand AddNewItemCommand { get; }

        /// <summary>
        /// Editable for the instance that would be added when the AddNewItemCommand gets executed.
        /// This is to edit the properties before actually adding the item to the collection.
        /// </summary>
        IEditableConfig ItemToAddEditable { get; }

        Type SubItemType { get; }
    }
}