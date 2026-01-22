using BethersaApp.Services;
using System.Windows;

namespace BethersaApp.Views;

public partial class SeleccionFacturaDialog : Window
{
    public TipoFactura? TipoSeleccionado { get; private set; }

    public SeleccionFacturaDialog()
    {
        InitializeComponent();
    }

    private void BtnFacturaA_Click(object sender, RoutedEventArgs e)
    {
        TipoSeleccionado = TipoFactura.FacturaA;
        DialogResult = true;
        Close();
    }

    private void BtnFacturaNegro_Click(object sender, RoutedEventArgs e)
    {
        TipoSeleccionado = TipoFactura.FacturaNegro;
        DialogResult = true;
        Close();
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        TipoSeleccionado = null;
        DialogResult = false;
        Close();
    }
}
