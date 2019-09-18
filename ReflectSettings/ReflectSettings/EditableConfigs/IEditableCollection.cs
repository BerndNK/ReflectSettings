using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace ReflectSettings.EditableConfigs
{
    public interface IEditableCollection : IEditableConfig, IEnumerable, INotifyCollectionChanged
    {
        ObservableCollection<IEditableConfig> SubEditables { get; }

        ICommand AddNewItemCommand { get; }

        ICommand RemoveItemCommand { get; }

        /// <summary>
        /// Editable for the instance that would be added when the AddNewItemCommand gets executed.
        /// This is to edit the properties before actually adding the item to the collection.
        /// </summary>
        IEditableConfig ItemToAddEditable { get; }

        Type SubItemType { get; }

        int ItemCount { get; }

        /// <summary>
        /// Property to store the selected value
        /// </summary>
        object SelectedValue { get; set; }
    }
}