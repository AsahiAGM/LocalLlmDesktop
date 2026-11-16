using System;
using System.Globalization;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Data.Converters;

namespace LocalLlmDesktop.Views.Converters;

public class RoleToAlignmentConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if(value is string role && role == "user")
        {
            return Avalonia.Layout.HorizontalAlignment.Right;
        }

        return Avalonia.Layout.HorizontalAlignment.Left;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}