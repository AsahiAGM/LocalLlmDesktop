using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LocalLlmDesktop.Views.Converters;

public class RoleToBrushConverter : IValueConverter
{
    private static readonly IBrush UserBrush = 
        new SolidColorBrush(Color.Parse("#245B91"));
    
    private static readonly IBrush AssistantBrush = 
        new SolidColorBrush(Color.Parse("#303030"));

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if(value is string role && role == "user")
        {
            return UserBrush;
        }

        return AssistantBrush;
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