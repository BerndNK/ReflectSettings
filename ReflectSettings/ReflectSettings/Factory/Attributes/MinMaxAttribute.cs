using System;

namespace ReflectSettings.Factory.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MinMaxAttribute : Attribute
    {
        public dynamic Min { get; set; }
        public dynamic Max { get; set; }

        public MinMaxAttribute(int min, int max)
        {
            Min = min;
            Max = max;
            SwapIfNecessary();
        }

        public MinMaxAttribute(double min, double max)
        {
            Min = min;
            Max = max;
            SwapIfNecessary();
        }

        public MinMaxAttribute(float min, float max)
        {
            Min = min;
            Max = max;
            SwapIfNecessary();
        }

        private void SwapIfNecessary()
        {
            if(Min > Max)
            {
                var tmp = Min;
                Min = Max;
                Max = tmp;
            }
        }

        public MinMaxAttribute() : this(0, int.MaxValue)
        {}
    }
}
