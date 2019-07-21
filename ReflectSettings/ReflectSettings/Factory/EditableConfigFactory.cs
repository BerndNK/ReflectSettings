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
            var editableProperties = EditableProperties(configurable.GetType());
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
            var editableType = EditableType(propertyInfo);

            object instance = null;
            try
            {
                if (IsComplex(propertyInfo) && HasCyclicTypes(propertyInfo, new[] {configurable.GetType()}))
                    throw new Exception("Cyclic type dependency detected. This is currently not supported.");

                instance = Activator.CreateInstance(editableType, configurable, propertyInfo, this);
            }
            catch (Exception e)
            {
                Debug.Fail($"Failed to create instance of type {editableType}. Exception: {e}");
            }

            if (instance is IEditableConfig editableConfig)
                return editableConfig;

            Debug.Fail($"Failed to cast type {editableType} to {typeof(IEditableConfig)}.");
            return new EditableDummy(configurable, propertyInfo, this);
        }

        private bool HasCyclicTypes(PropertyInfo propertyInfo, IList<Type> previousTypes)
        {
            var targetType = propertyInfo.PropertyType;
            var subTypes = EditableProperties(targetType);

            foreach (var subType in subTypes.Where(IsComplex))
            {
                if (previousTypes.Any(t => t == subType.PropertyType))
                    return true;

                if (HasCyclicTypes(subType, previousTypes.Concat(new[] {subType.PropertyType}).ToList()))
                    return true;
            }

            return false;
        }

        private bool IsComplex(PropertyInfo propertyInfo) => propertyInfo.PropertyType.IsClass;

        private Type EditableType(PropertyInfo propertyInfo)
        {
            if (!TypeToEditableConfig.TryGetValue(propertyInfo.PropertyType, out var editableType))
            {
                if (propertyInfo.PropertyType.IsEnum)
                    editableType = typeof(EditableEnum<>).MakeGenericType(propertyInfo.PropertyType);
                else
                    editableType = typeof(EditableComplex<>).MakeGenericType(propertyInfo.PropertyType);
            }

            return editableType;
        }

        private IEnumerable<PropertyInfo> EditableProperties(Type ofType)
        {
            var allProperties = ofType.GetProperties();

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