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
        private IEditableConfig _itemToAddEditable;

        public EditableCollection(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory) : base(
            forInstance, propertyInfo, factory)
        {
            Value = Value;
            AddNewItemCommand = new DelegateCommand(AddNewItem);
            PrepareItemToAdd();
        }

        public ICommand AddNewItemCommand { get; }

        public IEditableConfig ItemToAddEditable
        {
            get => _itemToAddEditable;
            private set
            {
                _itemToAddEditable = value; 
                OnPropertyChanged();
            }
        }

        private void AddNewItem()
        {
            Add((TItem) ItemToAddEditable.Value);
            PrepareItemToAdd();
        }

        private void PrepareItemToAdd()
        {
            var instanceToAdd = InstantiateObject<TItem>();
            ItemToAddEditable = EditableConfigFor(instanceToAdd);
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
            var newInstance = InstantiateObject<TCollection>();
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

        private ICollection<TItem> AsCollection => Value as ICollection<TItem> ?? new List<TItem>();

        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => AsCollection.GetEnumerator();

        public IEnumerator GetEnumerator() => AsCollection.GetEnumerator();

        public void Add(TItem item)
        {
            try
            {
                AsCollection.Add(item);
            }
            catch (Exception)
            {
                // ignored. Exception may occur for example, when an item with duplicate key gets added to a dictionary.
                return;
            }

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

        public ObservableCollection<IEditableConfig> SubEditables { get; private set; } =
            new ObservableCollection<IEditableConfig>();

        private IEditableConfig EditableConfigFor(TItem item)
        {
            var config = Factory.Reflect(item, true).First();
            config.ChangeTrackingManager = ChangeTrackingManager;
            config.InheritedCalculatedValuesAttribute.AddRange(AllCalculatedValuesAttributeForChildren);
            config.UpdateCalculatedValues();
            return config;
        }

        public override void UpdateCalculatedValues()
        {
            base.UpdateCalculatedValues();
            foreach (var editable in SubEditables)
            {
                editable.InheritedCalculatedValuesAttribute.AddRange(AllCalculatedValuesAttributeForChildren.Except(editable.InheritedCalculatedValuesAttribute));
                editable.UpdateCalculatedValues();
            }

            if (ItemToAddEditable == null)
                return;

            ItemToAddEditable.InheritedCalculatedValuesAttribute.AddRange(AllCalculatedValuesAttributeForChildren.Except(ItemToAddEditable.InheritedCalculatedValuesAttribute));
            ItemToAddEditable.UpdateCalculatedValues();
        }
    }
}