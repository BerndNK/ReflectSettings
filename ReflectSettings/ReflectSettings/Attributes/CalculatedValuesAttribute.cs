using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CalculatedValuesAttribute : BaseKeyValueAttribute
    {
        public bool ForCollectionEntries { get; }

        /// <summary>
        /// Values for the property of this attribute are calculated by the current instance of the parent object.
        /// </summary>
        /// <param name="memberMethodName">Expects a member method name with an empty argument list. Gets as input parameter the current instance, which contains the property this Attribute is attached to.</param>
        /// <param name="forCollectionEntries">Whether this attributes counts for the property itself or for its child values in case of a collection</param>
        public CalculatedValuesAttribute(string memberMethodName, bool forCollectionEntries = false) : base(
            memberMethodName, forCollectionEntries)
        {
            ForCollectionEntries = forCollectionEntries;
        }


        /// <summary>
        /// Values for the property of this attribute are calculated by the current instance of the parent object.
        /// </summary>
        /// <param name="memberMethodName">Expects a member method name with an empty argument list. Gets as input parameter the current instance, which contains the property this Attribute is attached to.</param>
        /// <param name="key">Give this method an additional key. You can use this Key to access this method, when you're within a subtype of a property this attribute is defined at. Use the key as member name then.</param>
        public CalculatedValuesAttribute(string memberMethodName, string key) : base(memberMethodName, key)
        {
        }

        public CalculatedValuesAttribute() : this("")
        {
        }

        public IEnumerable<object> ResolveValues(IReadOnlyList<CalculatedValuesAttribute> inheritedAttributes = null)
        {
            try
            {
                var (method, attachedInstance) = ResolveMethodToCall(inheritedAttributes);
                if (method == null || attachedInstance == null)
                    return Enumerable.Empty<object>();

                var result = InvokeMethod(attachedInstance, method);
                if (result == null)
                    return Enumerable.Empty<object>();
                else
                    return result;
            }
            catch (Exception e)
            {
                Debug.Fail(
                    $"Failed to call method of instance: {AttachedToInstance} with method name: {MemberMethodName}. Exception: {e}");
            }

            return Enumerable.Empty<object>();
        }

        private IEnumerable<object> InvokeMethod(object fromInstance, MethodInfo method)
        {
            var enumerable = method.Invoke(fromInstance, new object[0]) as IEnumerable;
            if (enumerable == null)
                return Enumerable.Empty<object>();

            return enumerable.Cast<object>();
        }
    }
}