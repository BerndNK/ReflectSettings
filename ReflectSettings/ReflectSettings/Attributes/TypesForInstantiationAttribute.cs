using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TypesForInstantiationAttribute : Attribute
    {
        public IEnumerable<Type> Types { get; }

        public TypesForInstantiationAttribute(params Type[] typesForInstantiation)
        {
            if (typesForInstantiation == null)
                Types = new List<Type>();
            else
                Types = typesForInstantiation.ToList();
        }
    }
}