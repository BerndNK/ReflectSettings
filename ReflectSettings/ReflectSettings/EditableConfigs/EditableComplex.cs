using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableComplex<T> : EditableConfigBase<T>, IEditableComplex where T : class
    {
        protected override T ParseValue(object value)
        {
            if (value is T asT)
            {
                if (IsValueAllowed(asT))
                {
                    CreateSubEditables(asT);
                    return asT;
                }
                else if (Value is T currentValue && IsValueAllowed(currentValue))
                {
                    return currentValue;
                }
            }

            // if null is allowed, return null
            var predefinedValues = GetPredefinedValues().ToList();
            if (predefinedValues.Any(x => x == null))
                return null;

            if (predefinedValues.Any())
                return predefinedValues.First();

            // otherwise create a new instance
            var newInstance = InstantiateObject<T>();
            CreateSubEditables(newInstance);
            return newInstance;
        }

        private void CreateSubEditables(T fromInstance)
        {
            SubEditables.Clear();
            if (fromInstance != null)
            {
                var subEditables = Factory.Reflect(fromInstance).ToList();
                foreach (var item in subEditables)
                {
                    item.InheritedCalculatedValuesAttribute.AddRange(AllCalculatedValuesAttributeForChildren);
                    item.ChangeTrackingManager = ChangeTrackingManager;
                    if (item.IsDisplayNameProperty)
                        item.PropertyChanged += OnDisplayNameEditablePropertyChanged;
                    item.UpdateCalculatedValues();
                    SubEditables.Add(item);
                }
            }
        }

        private void OnDisplayNameEditablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(DisplayName));
        }

        public ObservableCollection<IEditableConfig> SubEditables { get; private set; } =
            new ObservableCollection<IEditableConfig>();

        protected override string ResolveDisplayName()
        {
            var subEditableThatIsDisplayName = SubEditables.FirstOrDefault(x => x.IsDisplayNameProperty);
            var displayName = subEditableThatIsDisplayName?.Value?.ToString();
            if (displayName != null)
                return displayName;

            displayName = Value?.ToString();
            if (displayName != null)
                return displayName;

            return base.ResolveDisplayName();
        }

        public EditableComplex(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory) : base(
            forInstance, propertyInfo, factory)
        {
            if (propertyInfo.PropertyType != typeof(T))
                throw new ArgumentException(
                    $"Given property was not type of T of this instance. Property type: {propertyInfo.PropertyType} T: {typeof(T)}");

            // parse the existing value on the instance
            Value = Value;
        }


        public override void UpdateCalculatedValues()
        {
            base.UpdateCalculatedValues();
            foreach (var editable in SubEditables)
            {
                editable.InheritedCalculatedValuesAttribute.AddRange(
                    AllCalculatedValuesAttributeForChildren.Except(editable.InheritedCalculatedValuesAttribute));
                editable.UpdateCalculatedValues();
            }
        }

        protected override void SetChangeTrackingManagerForChildren(ChangeTrackingManager value)
        {
            base.SetChangeTrackingManagerForChildren(value);
            foreach (var editable in SubEditables)
            {
                editable.ChangeTrackingManager = value;
            }
        }
    }
}