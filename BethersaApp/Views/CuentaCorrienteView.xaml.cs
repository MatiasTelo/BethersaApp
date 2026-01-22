using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BethersaApp.Views;

public partial class CuentaCorrienteView : UserControl
{
    public CuentaCorrienteView()
    {
        InitializeComponent();
    }
}

public class PositiveConverter : IValueConverter
{
    public static PositiveConverter Instance { get; } = new PositiveConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return d > 0;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
