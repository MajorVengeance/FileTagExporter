using System;
using System.Globalization;
using System.Windows.Data;
using FileTagExporter.Models;

namespace FileTagExporter.ValueConverters;


[ValueConversion(typeof(int), typeof(OverwriteBehavior))]
public class OverwriteBehaviorValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Enum.TryParse(typeof(OverwriteBehavior), value.ToString(), out var enumValue);
        var paramValue = Enum.Parse(typeof(OverwriteBehavior), parameter.ToString() ?? string.Empty);
        if ((OverwriteBehavior)enumValue! == (OverwriteBehavior)paramValue)
            return true;
        else
            return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter;
    }
}