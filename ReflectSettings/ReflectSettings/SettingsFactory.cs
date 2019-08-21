using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        public IEnumerable<IEditableConfig> Reflect(object configurable, out ChangeTrackingManager changeTrackingManager, bool useConfigurableItself = false)
        {
            changeTrackingManager = new ChangeTrackingManager();
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
                var trackingManager = changeTrackingManager;
                return editableProperties.Select(t => EditableConfigFromPropertyInfo(configurable, t, trackingManager));
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
                else if (ImplementsType(propertyInfo, typeof(KeyValuePair<,>),out var keyValuePairType))
                {
                    var subItemTypes = keyValuePairType.GenericTypeArguments;
                    
                    editableType = typeof(EditableKeyValuePair<,>).MakeGenericType(subItemTypes);
                }
                else if (ImplementsType(propertyInfo, typeof(ICollection<>),out var iCollectionType))
                {
                    var subItemType = iCollectionType.GenericTypeArguments.First();

                    editableType = typeof(EditableCollection<,>).MakeGenericType(subItemType, propertyInfo.PropertyType);
                }
                else if (ImplementsType(propertyInfo, typeof(IReadOnlyCollection<>),out var iReadOnlyCollectionType))
                {
                    var subItemType = iReadOnlyCollectionType.GenericTypeArguments.First();

                    editableType = typeof(ReadonlyEditableCollection<,>).MakeGenericType(subItemType, propertyInfo.PropertyType);
                }
                else
                    editableType = typeof(EditableComplex<>).MakeGenericType(propertyInfo.PropertyType);
            }

            return editableType;
        }

        private bool ImplementsType(PropertyInfo propertyInfo, Type genericTypeToCheck, out Type specificImplementationOfGeneric)
        {
            specificImplementationOfGeneric = null;
            if (!genericTypeToCheck.IsGenericType)
                return propertyInfo.PropertyType.IsAssignableFrom(genericTypeToCheck);

            var isImplementationItself = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == genericTypeToCheck;
            var inheritedInterfaceType = 
                propertyInfo.PropertyType.GetInterfaces()
                .FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == genericTypeToCheck);

            // the given interface may be an generic definition like ICollection<>.
            // So in order to actually get the type of T (in ICollection<T>), the actual implemented type is provided as an out variable
            specificImplementationOfGeneric = 
                isImplementationItself
                    ? propertyInfo.PropertyType
                    : inheritedInterfaceType;

            return isImplementationItself || inheritedInterfaceType != null;
        }

        private IEnumerable<PropertyInfo> EditableProperties(Type ofType)
        {
            var allProperties = ofType.GetProperties();

            return allProperties.Where(HasGetterAndSetter).Where(IsNotIgnored).Where(IsNotStatic);
        }

        private bool IsNotStatic(PropertyInfo arg)
        {
            return !arg.GetAccessors(false).Any(x => x.IsStatic);
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