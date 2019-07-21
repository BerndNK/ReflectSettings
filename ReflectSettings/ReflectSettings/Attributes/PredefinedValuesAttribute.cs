using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PredefinedValuesAttribute : Attribute
    {
        public IList<object> Values { get; }

        public PredefinedValuesAttribute(params object[] predefinedValues)
        {
            if (predefinedValues == null)
                Values = new List<object> {null};
            else
                Values = predefinedValues.ToList();
        }

        public PredefinedValuesAttribute()
        {
            Values = new List<object>();
        }
    }
}