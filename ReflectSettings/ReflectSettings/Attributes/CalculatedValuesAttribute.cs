using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CalculatedValuesAttribute : Attribute
    {
        public string Key { get; }
        private readonly string _memberMethodName;

        /// <summary>
        /// Values for the property of this attribute are calculated by the current instance of the parent object.
        /// </summary>
        /// <param name="memberMethodName">Expects a member method name with an empty argument list. Gets as input parameter the current instance, which contains the property this Attribute is attached to.</param>
        public CalculatedValuesAttribute(string memberMethodName)
        {
            _memberMethodName = memberMethodName;
        }


        /// <summary>
        /// Values for the property of this attribute are calculated by the current instance of the parent object.
        /// </summary>
        /// <param name="memberMethodName">Expects a member method name with an empty argument list. Gets as input parameter the current instance, which contains the property this Attribute is attached to.</param>
        /// <param name="key">Give this method an additional key. You can use this Key to access this method, when you're within a subtype of a property this attribute is defined at. Use the key as member name then.</param>
        public CalculatedValuesAttribute(string memberMethodName, string key)
        {
            Key = key;
            _memberMethodName = memberMethodName;
        }

        public CalculatedValuesAttribute() : this("")
        {
        }

        public object AttachedToInstance { get; set; }

        public IEnumerable<object> CallMethod(IList<CalculatedValuesAttribute> inheritedAttributes = null)
        {
            if (AttachedToInstance == null)
            {
                Debug.Fail($"{nameof(AttachedToInstance)} was not set. Set it before calling CallMethod in {this}");
                return Enumerable.Empty<object>();
            }

            if (string.IsNullOrWhiteSpace(_memberMethodName))
                return Enumerable.Empty<object>();

            if(inheritedAttributes == null)
                inheritedAttributes = new List<CalculatedValuesAttribute>();

            var targetMethod = TargetMethod(AttachedToInstance);

            if (targetMethod == null)
            {
                var matchingAttribute = inheritedAttributes.FirstOrDefault(x => x.Key == _memberMethodName && !Equals(x, this));
                if (matchingAttribute != null)
                    return matchingAttribute.CallMethod();
                return Enumerable.Empty<object>();
            }

            try
            {
                var result = targetMethod();
                if (result == null)
                    return Enumerable.Empty<object>();
                else
                    return targetMethod();
            }
            catch (Exception e)
            {
                Debug.Fail(
                    $"Failed to call method of instance: {AttachedToInstance} with method name: {_memberMethodName}. Exception: {e}");
            }

            return Enumerable.Empty<object>();
        }

        private Func<IEnumerable<object>> TargetMethod(object fromInstance)
        {
            var type = fromInstance.GetType();
            var matchingMethod = type.GetMethod(_memberMethodName);
            if (matchingMethod == null)
                return null;

            return () =>
            {
                var enumerable = matchingMethod.Invoke(fromInstance, new object[0]) as IEnumerable;
                if (enumerable == null)
                    return Enumerable.Empty<object>();

                return enumerable.Cast<object>();
            };
        }
    }
}