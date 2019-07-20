using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflectSettings.Factory.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PredefinedValuesAttribute : Attribute
    {
        public IList<object> Values { get; }

        public PredefinedValuesAttribute(params object[] predefinedValues)
        {
            Values = predefinedValues.ToList();
        }

        public PredefinedValuesAttribute(Func<IEnumerable<object>> predefinedValuesResolver)
        {
            Values = predefinedValuesResolver().ToList();
        }
    }
}
