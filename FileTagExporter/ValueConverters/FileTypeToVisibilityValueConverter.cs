using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FileTagExporter.Models;

namespace FileTagExporter.ValueConverters;

[ValueConversion(typeof(FileType), typeof(Visibility))]
public class FileTypeToVisibilityValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Enum.TryParse(typeof(FileType), value.ToString(), out var enumValue);
        var paramValue = Enum.Parse(typeof(FileType), parameter.ToString() ?? string.Empty);
        if ((FileType)enumValue! == (FileType)paramValue)
            return Visibility.Visible;
        else
            return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter;
    }
}