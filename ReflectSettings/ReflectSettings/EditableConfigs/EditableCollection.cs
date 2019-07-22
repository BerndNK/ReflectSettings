using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace ReflectSettings.EditableConfigs
{
    public class EditableCollection<TItem, TCollection> : EditableConfigBase<TCollection>, IEditableCollection,
        ICollection<TItem> where TCollection : class, ICollection<TItem>
    {
        public EditableCollection(object forInstance, PropertyInfo propertyInfo, EditableConfigFactory factory) : base(
            forInstance, propertyInfo, factory)
        {
            Value = Value;
            AddNewItemCommand = new DelegateCommand(AddNewItem);
        }

        public ICommand AddNewItemCommand { get; set; }

        private void AddNewItem()
        {
            Add(Activator.CreateInstance<TItem>());
        }

        protected override TCollection ParseValue(object value)
        {
            if (value is TCollection asT)
            {
                if (IsValueAllowed(asT))
                {
                    CreateSubEditables(asT);
                    return asT;
                }
                else if (Value is TCollection currentValue && IsValueAllowed(currentValue))
                {
                    return currentValue;
                }
            }

            // if null is allowed, return null
            if (GetPredefinedValues().Any(x => x == null))
                return null;

            // otherwise create a new instance
            var newInstance = InstantiateObject();
            CreateSubEditables(newInstance);
            return newInstance;
        }

        private void CreateSubEditables(TCollection collection)
        {
            SubEditables.Clear();
            foreach (var editableConfig in collection.Select(EditableConfigFor))
            {
                SubEditables.Add(editableConfig);
            }
        }

        private TCollection InstantiateObject()
        {
            return Activator.CreateInstance<TCollection>();
        }

        private ICollection<TItem> AsCollection => Value as ICollection<TItem> ?? new List<TItem>();

        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => AsCollection.GetEnumerator();

        public IEnumerator GetEnumerator() => AsCollection.GetEnumerator();

        public void Add(TItem item)
        {
            AsCollection.Add(item);
            SubEditables.Add(EditableConfigFor(item));
            OnPropertyChanged(nameof(SubEditables));
        }

        public void Clear()
        {
            AsCollection.Clear();
            SubEditables.Clear();
            OnPropertyChanged(nameof(SubEditables));
        }

        public bool Contains(TItem item) => AsCollection.Contains(item);

        public void CopyTo(TItem[] array, int arrayIndex) => AsCollection.CopyTo(array, arrayIndex);

        public bool Remove(TItem item)
        {
            if (AsCollection.Remove(item))
            {
                SubEditables.Remove(SubEditables.First(x => x.Value.Equals(item)));
                OnPropertyChanged(nameof(SubEditables));
                return true;
            }
            else
                return false;
        }

        public int Count => AsCollection.Count;

        public bool IsReadOnly => AsCollection.IsReadOnly;

        public ObservableCollection<IEditableConfig> SubEditables { get; private set; } = new ObservableCollection<IEditableConfig>();

        private IEditableConfig EditableConfigFor(TItem item)
        {
            var config = Factory.Produce(item, true).First();
            config.ChangeTrackingManager = ChangeTrackingManager;
            return config;
        }
    }
}