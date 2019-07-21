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
                if (propertyInfo.PropertyType.IsEnum)
                    editableType = typeof(EditableEnum<>).MakeGenericType(propertyInfo.PropertyType);
                else
                    editableType = typeof(EditableComplex);
            }

            object instance = null;
            try
            {
                instance = Activator.CreateInstance(editableType, configurable, propertyInfo);
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