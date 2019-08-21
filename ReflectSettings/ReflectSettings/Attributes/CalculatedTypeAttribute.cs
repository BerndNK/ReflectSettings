using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CalculatedTypeAttribute : Attribute, INeedsInstance
    {
        public bool ForCollectionEntries { get; }
        public string Key { get; }
        private readonly string _memberMethodName;

        /// <summary>
        /// Type for the property of this attribute are calculated by the current instance of the parent object.
        /// </summary>
        /// <param name="memberMethodName">Expects a member method name with an empty argument list. Gets as input parameter the current instance, which contains the property this Attribute is attached to.</param>
        /// <param name="forCollectionEntries">Whether this attributes counts for the property itself or for its child values in case of a collection</param>
        public CalculatedTypeAttribute(string memberMethodName, bool forCollectionEntries = false)
        {
            ForCollectionEntries = forCollectionEntries;
            _memberMethodName = memberMethodName;
        }


        /// <summary>
        /// Type for the property of this attribute are calculated by the current instance of the parent object.
        /// </summary>
        /// <param name="memberMethodName">Expects a member method name with an empty argument list. Gets as input parameter the current instance, which contains the property this Attribute is attached to.</param>
        /// <param name="key">Give this method an additional key. You can use this Key to access this method, when you're within a subtype of a property this attribute is defined at. Use the key as member name then.</param>
        public CalculatedTypeAttribute(string memberMethodName, string key)
        {
            Key = key;
            _memberMethodName = memberMethodName;
        }

        public CalculatedTypeAttribute() : this("")
        {
        }

        public object AttachedToInstance { get; set; }

        public Type CallMethod(IList<CalculatedTypeAttribute> inheritedAttributes = null, object parameter = null)
        {
            if (AttachedToInstance == null)
            {
                Debug.Fail($"{nameof(AttachedToInstance)} was not set. Set it before calling CallMethod in {this}");
                return typeof(object);
            }

            if (string.IsNullOrWhiteSpace(_memberMethodName))
                return typeof(object);

            if (inheritedAttributes == null)
                inheritedAttributes = new List<CalculatedTypeAttribute>();

            var targetMethod = TargetMethod(AttachedToInstance, parameter);

            if (targetMethod == null)
            {
                var matchingAttribute =
                    inheritedAttributes.FirstOrDefault(x => x.Key == _memberMethodName && !Equals(x, this));
                if (matchingAttribute != null)
                    return matchingAttribute.CallMethod(null, parameter);
                return typeof(object);
            }

            try
            {
                var result = targetMethod();
                if (result == null)
                    return typeof(object);
                else
                    return result;
            }
            catch (Exception e)
            {
                Debug.Fail(
                    $"Failed to call method of instance: {AttachedToInstance} with method name: {_memberMethodName}. Exception: {e}");
            }

            return typeof(object);
        }

        private Func<Type> TargetMethod(object fromInstance, object parameter)
        {
            var type = fromInstance.GetType();
            var matchingMethod = type.GetMethod(_memberMethodName);
            if (matchingMethod == null)
                return null;

            return () =>
            {
                var resolvedType = matchingMethod.Invoke(fromInstance, new[] {parameter}) as Type;
                if (resolvedType == null)
                    return typeof(object);

                return resolvedType;
            };
        }
    }
}