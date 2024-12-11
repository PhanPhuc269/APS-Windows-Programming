using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace App.Converters;

public class LowQuantityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isLowQuantity)
        {
            return isLowQuantity ? new SolidColorBrush(Microsoft.UI.Colors.Red) : new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
