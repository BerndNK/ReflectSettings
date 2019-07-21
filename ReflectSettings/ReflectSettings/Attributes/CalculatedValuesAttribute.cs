using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CalculatedValuesAttribute : Attribute
    {
        private readonly string _memberMethodName;

        /// <summary>
        /// Values for the property of this attribute are calculated by the current instance of the parent object.
        /// </summary>
        /// <param name="memberMethodName">Expects a member method name with an empty argument list. Gets as input parameter the current instance, which contains the property this Attribute is attached to.</param>
        public CalculatedValuesAttribute(string memberMethodName)
        {
            _memberMethodName = memberMethodName;
        }

        public CalculatedValuesAttribute() : this("")
        {}

        public IEnumerable<object> CallMethod(object fromInstance)
        {
            if (string.IsNullOrWhiteSpace(_memberMethodName))
                return Enumerable.Empty<object>();

            var targetMethod = TargetMethod(fromInstance);

            if (targetMethod == null)
                return Enumerable.Empty<object>();

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
                    $"Failed to call method of instance: {fromInstance} with method name: {_memberMethodName}. Exception: {e}");
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