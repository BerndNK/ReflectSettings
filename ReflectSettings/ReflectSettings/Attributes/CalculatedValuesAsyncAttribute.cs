using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CalculatedValuesAsyncAttribute : BaseKeyValueAttribute
    {
        public CalculatedValuesAsyncAttribute(string memberMethodName, bool forCollectionEntries = false) : base(
            memberMethodName, forCollectionEntries)
        {
        }

        public CalculatedValuesAsyncAttribute(string memberMethodName, string key) : base(memberMethodName, key)
        {
        }

        public CalculatedValuesAsyncAttribute() : this("")
        {
        }

        public async Task<IEnumerable<object>> ResolveValuesAsync(
            IReadOnlyList<CalculatedValuesAsyncAttribute> inheritedAttributes = null)
        {
            try
            {
                var (method, attachedInstance) = ResolveMethodToCall(inheritedAttributes);
                if (method == null || attachedInstance == null)
                    return Enumerable.Empty<object>();

                var result = await InvokeMethod(attachedInstance, method);
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

        private async Task<IEnumerable<object>> InvokeMethod(object fromInstance, MethodInfo method)
        {
            var asyncMethod = (Task<IEnumerable<object>>) method.Invoke(fromInstance, new object[0]);
            return await asyncMethod;
        }
    }
}