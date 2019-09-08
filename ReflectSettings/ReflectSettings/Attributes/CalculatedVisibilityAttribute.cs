using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CalculatedVisibilityAttribute : BaseKeyValueAttribute
    {
        public CalculatedVisibilityAttribute(string memberMethodName, bool forCollectionEntries = false) : base(
            memberMethodName, forCollectionEntries)
        {
        }

        public CalculatedVisibilityAttribute(string memberMethodName, string key) : base(memberMethodName, key)
        {
        }

        public CalculatedVisibilityAttribute() : this("")
        {
        }

        public bool IsHidden(IReadOnlyList<CalculatedVisibilityAttribute> inheritedAttributes = null)
        {
            try
            {
                var (method, attachedInstance) = ResolveMethodToCall(inheritedAttributes);
                if (method == null || attachedInstance == null)
                    return false;

                return InvokeMethod(method, attachedInstance);
            }
            catch (Exception e)
            {
                Debug.Fail(
                    $"Failed to call method of instance: {AttachedToInstance} with method name: {MemberMethodName}. Exception: {e}");
            }

            return false;
        }


        private bool InvokeMethod(MethodInfo method, object fromInstance)
        {
            var result = method.Invoke(fromInstance, new object[] { });
            if (result is bool asBool)
                return asBool;

            return false;
        }
    }
}