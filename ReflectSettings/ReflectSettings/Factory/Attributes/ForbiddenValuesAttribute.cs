using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflectSettings.Factory.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForbiddenValuesAttribute : Attribute
    {
        public IList<object> ForbiddenValues { get; set; }

        public ForbiddenValuesAttribute(params object[] forbiddenValues)
        {
            if (forbiddenValues == null)
                ForbiddenValues = new List<object>();
            else
                ForbiddenValues = forbiddenValues.ToList();
        }

        public ForbiddenValuesAttribute()
        {
            ForbiddenValues = new List<object>();
        }
    }
}