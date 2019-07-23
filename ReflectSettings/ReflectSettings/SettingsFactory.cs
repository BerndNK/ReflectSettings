using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettings
{
    public class SettingsFactory
    {
        private class InstanceWrapper<T>
        {
            public InstanceWrapper(T value)
            {
                Value = value;
            }

            public T Value { get; set; }
        }
        
        /// <summary>
        /// Creates IEditableConfig for each public get and set-able property of the given instance.
        /// Optionally gives a IEditableConfig for the given instance itself, instead of its properties.
        /// </summary>
        public IEnumerable<IEditableConfig> Reflect(object configurable, bool useConfigurableItself = false)
        {
            var changeTrackingManager = new ChangeTrackingManager();
            if (useConfigurableItself)
            {
                var wrapperType = typeof(InstanceWrapper<>).MakeGenericType(configurable.GetType());
                var wrapper = Activator.CreateInstance(wrapperType, configurable);
                return new List<IEditableConfig>
                    {EditableConfigFromPropertyInfo(wrapper, wrapper.GetType().GetProperty(nameof(InstanceWrapper<int>.Value)), changeTrackingManager)};
            }
            else
            {
                var editableProperties = EditableProperties(configurable.GetType());
                return editableProperties.Select(t => EditableConfigFromPropertyInfo(configurable, t, changeTrackingManager));
            }
        }

        public IDictionary<Type, Type> TypeToEditableConfig { get; } = new Dictionary<Type, Type>
        {
            {typeof(int), typeof(EditableInt)},
            {typeof(double), typeof(EditableDouble)},
            {typeof(string), typeof(EditableString)},
            {typeof(bool), typeof(EditableBool)}
        };

        private IEditableConfig EditableConfigFromPropertyInfo(object configurable, PropertyInfo propertyInfo, ChangeTrackingManager changeTrackingManager)
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
            {
                editableConfig.ChangeTrackingManager = changeTrackingManager;
                return editableConfig;
            }

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
                else if (IsCollection(propertyInfo))
                {
                    var subItemType = propertyInfo.PropertyType.GenericTypeArguments.First();

                    editableType =
                        typeof(EditableCollection<,>).MakeGenericType(subItemType, propertyInfo.PropertyType);
                }
                else
                    editableType = typeof(EditableComplex<>).MakeGenericType(propertyInfo.PropertyType);
            }

            return editableType;
        }

        private bool IsCollection(PropertyInfo propertyInfo)
        {
            var isICollectionItself = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
            var inheritsICollection = propertyInfo.PropertyType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));

            return isICollectionItself || inheritsICollection;
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