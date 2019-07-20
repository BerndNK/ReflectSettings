using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ReflectSettings.Factory.Attributes;
using ReflectSettings.Factory.EditableConfigs;

namespace ReflectSettings.Factory
{
    public class EditableConfigFactory
    {
        public IEnumerable<IEditableConfig> Produce(object configurable)
        {
            var editableProperties = EditableProperties(configurable);
            return editableProperties.Select(t => EditableConfigFromPropertyInfo(configurable, t));
        }

        public IDictionary<Type, Type> TypeToEditableConfig { get; } = new Dictionary<Type, Type>
        {
            {typeof(int), typeof(EditableInt)},
            {typeof(double), typeof(EditableDouble)},
            {typeof(string), typeof(EditableString)}
        };

        private IEditableConfig EditableConfigFromPropertyInfo(object configurable, PropertyInfo propertyInfo)
        {
            if (!TypeToEditableConfig.TryGetValue(propertyInfo.PropertyType, out var editableType))
            {
                editableType = typeof(EditableComplex);
            }

            object instance = null;
            try
            {
                instance = Activator.CreateInstance(editableType, configurable);
            }
            catch (Exception e)
            {
                Debug.Fail($"Failed to create instance of type {editableType}. Exception: {e}");
            }

            if (instance is IEditableConfig editableConfig)
                return editableConfig;

            Debug.Fail($"Failed to cast type {editableType} to {typeof(IEditableConfig)}.");
            return new EditableDummy(configurable, propertyInfo);
        }

        private IEnumerable<PropertyInfo> EditableProperties(object configurable)
        {
            var type = configurable.GetType();
            var allProperties = type.GetProperties();

            return allProperties.Where(HasGetterAndSetter).Where(IsNotIgnored);
        }

        private bool HasGetterAndSetter(PropertyInfo arg)
        {
            return arg.GetGetMethod() != null && arg.GetSetMethod() != null;
        }

        private bool IsNotIgnored(PropertyInfo arg)
        {
            var ignoredAttribute = arg.GetCustomAttributes(true).OfType<IgnoredForConfigAttribute>().FirstOrDefault();
            return ignoredAttribute == null;
        }
    }
}

namespace ReflectSettings.Factory.EditableConfigs
{
    public class EditableDummy : EditableConfigBase<object>
    {
        public EditableDummy(object forInstance, PropertyInfo propertyInfo) : base(forInstance, propertyInfo)
        {
        }

        protected override object ParseValue(object value) => value;
    }

    public class EditableInt : EditableConfigBase<int>
    {
        protected override int ParseValue(object value)
        {
            throw new NotImplementedException();
        }

        public EditableInt(object forInstance, PropertyInfo propertyInfo) : base(forInstance, propertyInfo)
        {
        }
    }

    public class EditableDouble : EditableConfigBase<double>
    {
        protected override double ParseValue(object value)
        {
            throw new NotImplementedException();
        }

        public EditableDouble(object forInstance, PropertyInfo propertyInfo) : base(forInstance, propertyInfo)
        {
        }
    }

    public class EditableString : EditableConfigBase<string>
    {
        protected override string ParseValue(object value)
        {
            throw new NotImplementedException();
        }

        public EditableString(object forInstance, PropertyInfo propertyInfo) : base(forInstance, propertyInfo)
        {
        }
    }

    public class EditableComplex : EditableConfigBase<object>
    {
        protected override object ParseValue(object value)
        {
            throw new NotImplementedException();
        }

        public EditableComplex(object forInstance, PropertyInfo propertyInfo) : base(forInstance, propertyInfo)
        {
        }
    }

    public interface IEditableConfig
    {
        PropertyInfo PropertyInfo { get; set; }

        object Value { get; set; }
    }

    public abstract class EditableConfigBase<T> : IEditableConfig
    {
        private readonly IList<Attribute> _attributes;

        public object ForInstance { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public object Value
        {
            get => GetValue();
            set => SetValue(ParseValue(value));
        }

        protected abstract T ParseValue(object value);

        private T GetValue() => (T) PropertyInfo.GetValue(ForInstance);

        private void SetValue(T value) => PropertyInfo.SetValue(ForInstance, value);

        protected EditableConfigBase(object forInstance, PropertyInfo propertyInfo)
        {
            ForInstance = forInstance;
            PropertyInfo = propertyInfo;

            _attributes = propertyInfo.GetCustomAttributes(true).OfType<Attribute>().ToList();
        }

        private TAttribute Attribute<TAttribute>() where TAttribute : Attribute =>
            _attributes.OfType<TAttribute>().FirstOrDefault() ?? Activator.CreateInstance<TAttribute>();

        protected MinMaxAttribute MinMax() => Attribute<MinMaxAttribute>();

        protected IEnumerable<T> PredefinedValues()
        {
            var staticValues = Attribute<PredefinedValuesAttribute>();
            var calculatedValuesAttribute = Attribute<CalculatedValuesAttribute>();

            var calculatedValues = calculatedValuesAttribute.CallMethod(ForInstance);

            return staticValues.Values.Concat(calculatedValues).OfType<T>();
        }
    }
}