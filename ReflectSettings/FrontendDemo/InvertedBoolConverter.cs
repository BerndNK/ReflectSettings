using System;
using System.Globalization;
using System.Windows.Data;

namespace FrontendDemo
{
    internal class InvertedBoolConverter : IValueConverter
    {
        public static InvertedBoolConverter Instance { get; } = new InvertedBoolConverter();

        private InvertedBoolConverter()
        {
            
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool asBool)
                return !asBool;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
