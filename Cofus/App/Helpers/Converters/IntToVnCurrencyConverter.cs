using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace App.Converters;

public class IntToVnCurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int number)
        {
            CultureInfo vietnameseCulture = new CultureInfo("vi-VN");
            string formattedAmount = number.ToString("C0", vietnameseCulture);
            return formattedAmount;
        }
        return "N/A";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
