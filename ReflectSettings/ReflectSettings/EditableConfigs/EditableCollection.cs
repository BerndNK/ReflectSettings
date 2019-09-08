using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace ReflectSettings.EditableConfigs
{
    public class EditableCollection<TItem, TCollection> : EditableConfigBase<TCollection>, IEditableCollection,
        ICollection<TItem> where TCollection : class, ICollection<TItem>
    {
        private IEditableConfig _itemToAddEditable;
        private object _additionalData;

        public EditableCollection(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory) : base(
            forInstance, propertyInfo, factory)
        {
            Value = Value;
            AddNewItemCommand = new DelegateCommand(AddNewItem);
            RemoveItemCommand = new DelegateCommand(RemoveItem);
            PrepareItemToAdd();
        }

        public TItem SelectedItem { get; set; }

        public object SelectedValue
        {
            get => SelectedItem;
            set
            {
                if (value is TItem asItem)
                    SelectedItem = asItem;
            }
        }

        public ICommand AddNewItemCommand { get; }

        public ICommand RemoveItemCommand { get; }

        public IEditableConfig ItemToAddEditable
        {
            get => _itemToAddEditable;
            private set
            {
                _itemToAddEditable = value;
                OnPropertyChanged();
            }
        }

        public new object AdditionalData
        {
            get => _additionalData;
            set
            {
                _additionalData = value;
                foreach (var config in SubEditables)
                {
                    config.AdditionalData = value;
                }

                if (ItemToAddEditable != null)
                    ItemToAddEditable.AdditionalData = AdditionalData;

                OnPropertyChanged();
            }
        }

        public Type SubItemType => typeof(TItem);

        public int ItemCount => AsCollection?.Count ?? 0;

        private void AddNewItem()
        {
            var newItem = (TItem) ItemToAddEditable.Value;
            Add(newItem);
            PrepareItemToAdd();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<TItem> {newItem}));
        }

        private void RemoveItem(object parameter)
        {
            if (!(parameter is TItem asT))
                return;

            if (AsCollection.Contains(asT))
            {
                if(Remove(asT))
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<TItem> {asT}));
            }
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
            ClearSubEditables();
            foreach (var editableConfig in collection.Select(EditableConfigFor))
            {
                editableConfig.AdditionalData = AdditionalData;
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

            var editable = EditableConfigFor(item);
            if (typeof(TItem).IsPrimitive || typeof(TItem) == typeof(string) || editable is IEditableKeyValuePair)
                editable.ValueChanged += OnPrimitiveChildValueChanged;

            SubEditables.Add(editable);
            OnPropertyChanged(nameof(ItemCount));
            OnPropertyChanged(nameof(SubEditables));
        }

        private void OnPrimitiveChildValueChanged(object sender, EditableConfigValueChangedEventArgs e)
        {
            var oldValue = (TItem) e.OldValue;
            var newValue = (TItem) e.NewValue;

            var changedChild = (IEditableConfig) sender;
            var indexOfEditable = SubEditables.IndexOf(changedChild);

            if (Value is IList<TItem> asList)
                asList[indexOfEditable] = newValue;
            else
            {
                if (AsCollection.Contains(oldValue))
                {
                    AsCollection.Remove(oldValue);
                    AsCollection.Add(newValue);
                }
            }
        }

        private void ClearSubEditables()
        {
            foreach (var editable in SubEditables)
            {
                editable.ValueChanged -= OnPrimitiveChildValueChanged;
            }

            SubEditables.Clear();
        }

        public void Clear()
        {
            AsCollection.Clear();
            ClearSubEditables();
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
                OnPropertyChanged(nameof(ItemCount));
                OnPropertyChanged(nameof(Value));
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
            var config = Factory.Reflect(item, out _, true).First();
            config.ChangeTrackingManager = ChangeTrackingManager;
            config.InheritedCalculatedValuesAttribute.AddRange(AllCalculatedValuesAttributeForChildren);
            config.InheritedCalculatedTypeAttribute.AddRange(AllCalculatedTypeAttributeForChildren);
            config.UpdateCalculatedValues();
            config.AdditionalData = AdditionalData;
            return config;
        }

        public override void UpdateCalculatedValues()
        {
            base.UpdateCalculatedValues();
            foreach (var editable in SubEditables)
            {
                editable.InheritedCalculatedValuesAttribute.AddRange(
                    AllCalculatedValuesAttributeForChildren.Except(editable.InheritedCalculatedValuesAttribute));
                editable.InheritedCalculatedTypeAttribute.AddRange(
                    AllCalculatedTypeAttributeForChildren.Except(editable.InheritedCalculatedTypeAttribute));
                editable.UpdateCalculatedValues();
            }

            if (ItemToAddEditable == null)
                return;

            ItemToAddEditable.InheritedCalculatedValuesAttribute.AddRange(
                AllCalculatedValuesAttributeForChildren.Except(ItemToAddEditable.InheritedCalculatedValuesAttribute));
            ItemToAddEditable.InheritedCalculatedTypeAttribute.AddRange(
                AllCalculatedTypeAttributeForChildren.Except(ItemToAddEditable.InheritedCalculatedTypeAttribute));
            ItemToAddEditable.UpdateCalculatedValues();
        }

        protected override void SetChangeTrackingManagerForChildren(ChangeTrackingManager value)
        {
            base.SetChangeTrackingManagerForChildren(value);
            foreach (var editable in SubEditables)
            {
                editable.ChangeTrackingManager = value;
            }

            if (ItemToAddEditable == null)
                return;

            ItemToAddEditable.ChangeTrackingManager = value;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}