using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BethersaApp.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue;
        
        // Handle string values (for checking if string is not empty)
        if (value is string str)
        {
            boolValue = !string.IsNullOrWhiteSpace(str);
        }
        else
        {
            boolValue = value is bool b && b;
        }

        bool invert = parameter?.ToString()?.ToLower() == "inverse";

        if (invert)
            boolValue = !boolValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Handle string values (for placeholder visibility)
        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? Visibility.Visible : Visibility.Collapsed;
        }
        
        bool boolValue = value is bool b && b;
        return boolValue ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
