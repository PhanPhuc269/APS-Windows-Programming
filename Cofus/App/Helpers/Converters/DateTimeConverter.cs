using System;
using Microsoft.UI.Xaml.Data;

namespace App.Converters;

public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            // Định dạng ngày và giờ
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
