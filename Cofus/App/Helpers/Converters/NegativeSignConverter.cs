using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace App.Converters
{
    public class NegativeSignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int intValue)
            {
                return intValue > 0 ? "-" : string.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
