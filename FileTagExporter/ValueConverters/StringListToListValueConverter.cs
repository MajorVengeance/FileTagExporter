using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace FileTagExporter.ValueConverters;

[ValueConversion(typeof(List<string>), typeof(string))]
internal class StringListToStringValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.Join(';', (value as List<string>)!);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}