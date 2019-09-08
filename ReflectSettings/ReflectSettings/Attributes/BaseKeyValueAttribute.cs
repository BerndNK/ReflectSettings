using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ReflectSettings.Attributes
{
    /// <summary>
    /// Base class for attributes which provide inheriting features based on keys. The main functionality is to call a method to gain some sort of information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class BaseKeyValueAttribute : Attribute, INeedsInstance
    {
        public string Key { get; }
        protected string MemberMethodName { get; }
        public bool ForCollectionEntries { get; }

        /// <summary>
        /// Values for the property of this attribute are calculated by the current instance of the parent object.
        /// </summary>
        /// <param name="memberMethodName">Expects a member method name with an empty argument list. Gets as input parameter the current instance, which contains the property this Attribute is attached to.</param>
        /// <param name="forCollectionEntries">Whether this attributes counts for the property itself or for its child values in case of a collection</param>
        protected BaseKeyValueAttribute(string memberMethodName, bool forCollectionEntries = false)
        {
            MemberMethodName = memberMethodName;
            ForCollectionEntries = forCollectionEntries;
        }


        /// <summary>
        /// Values for the property of this attribute are calculated by the current instance of the parent object.
        /// </summary>
        /// <param name="memberMethodName">Expects a member method name with an empty argument list. Gets as input parameter the current instance, which contains the property this Attribute is attached to.</param>
        /// <param name="key">Give this method an additional key. You can use this Key to access this method, when you're within a subtype of a property this attribute is defined at. Use the key as member name then.</param>
        protected BaseKeyValueAttribute(string memberMethodName, string key)
        {
            Key = key;
            MemberMethodName = memberMethodName;
        }

        public object AttachedToInstance { get; set; }

        protected (MethodInfo Method, object AttatchedInstance) ResolveMethodToCall<T>(IReadOnlyList<T> inheritedAttributes = null) where T : BaseKeyValueAttribute
        {
            if (AttachedToInstance == null)
            {
                Debug.Fail($"{nameof(AttachedToInstance)} was not set. Set it before calling CallMethod in {this}");
                return default;
            }

            if (string.IsNullOrWhiteSpace(MemberMethodName))
                return default;

            if (inheritedAttributes == null)
                inheritedAttributes = new List<T>();

            var targetMethod = TargetMethodFromInstance(AttachedToInstance);

            // current instance does not have a method which matches the key. Look within inherited attributes for a matching key
            if (targetMethod != null)
                return (targetMethod, AttachedToInstance);

            var matchingAttribute = inheritedAttributes.FirstOrDefault(x => x.Key == MemberMethodName && !Equals(x, this));
            return matchingAttribute?.ResolveMethodToCall<T>() ?? default;
        }

        private MethodInfo TargetMethodFromInstance(object fromInstance)
        {
            var type = fromInstance.GetType();
            var matchingMethod = type.GetMethod(MemberMethodName);
            return matchingMethod;
        }
    }
}