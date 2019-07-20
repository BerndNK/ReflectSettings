using System;

namespace ReflectSettings.Factory.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MinMaxAttribute : Attribute
    {
        public dynamic Min { get; set; }
        public dynamic Max { get; set; }

        public MinMaxAttribute(int min = 0, int max = int.MaxValue)
        {
            Min = min;
            Max = max;
        }
        
        public MinMaxAttribute(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public MinMaxAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
