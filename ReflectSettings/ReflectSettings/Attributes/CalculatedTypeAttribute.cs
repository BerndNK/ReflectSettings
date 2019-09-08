using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CalculatedTypeAttribute : BaseKeyValueAttribute
    {
        public CalculatedTypeAttribute(string memberMethodName, bool forCollectionEntries = false) : base(
            memberMethodName, forCollectionEntries)
        {
        }

        public CalculatedTypeAttribute(string memberMethodName, string key) : base(memberMethodName, key)
        {
        }

        public CalculatedTypeAttribute() : this("")
        {
        }

        public Type CallMethod(IReadOnlyList<CalculatedTypeAttribute> inheritedAttributes = null, object parameter = null)
        {
            try
            {
                var (method, attachedInstance) = ResolveMethodToCall(inheritedAttributes);
                if (method == null || attachedInstance == null)
                    return typeof(object);

                var result = InvokeMethod(method, attachedInstance, parameter);
                if (result == null)
                    return typeof(object);
                else
                    return result;
            }
            catch (Exception e)
            {
                Debug.Fail(
                    $"Failed to call method of instance: {AttachedToInstance} with method name: {MemberMethodName}. Exception: {e}");
            }

            return typeof(object);
        }

        
        private Type InvokeMethod(MethodInfo method, object fromInstance, object parameter)
        {
            var resolvedType = method.Invoke(fromInstance, new[] {parameter}) as Type;
            if (resolvedType == null)
                return typeof(object);

            return resolvedType;
        }
    }
}