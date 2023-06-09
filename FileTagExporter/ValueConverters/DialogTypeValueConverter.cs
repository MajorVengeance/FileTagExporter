﻿using System;
using System.Globalization;
using System.Windows.Data;
using FileTagExporter.Models;

namespace FileTagExporter.ValueConverters;

[ValueConversion(typeof(int), typeof(FileType))]
public class FileTypeValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Enum.TryParse(typeof(FileType), value.ToString(), out var enumValue);
        var paramValue = Enum.Parse(typeof(FileType), parameter.ToString() ?? string.Empty);
        if ((FileType)enumValue! == (FileType)paramValue)
            return true;
        else
            return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter;
    }
}
