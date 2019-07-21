using System;

namespace ReflectSettings.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoredForConfigAttribute : Attribute
    {
    }
}
