using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BethersaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private string _currentSection = "Pedidos";

    private readonly PedidosViewModel _pedidosViewModel;
    private readonly FacturacionViewModel _facturacionViewModel;
    private readonly ClientesViewModel _clientesViewModel;
    private readonly ProductosViewModel _productosViewModel;
    private readonly CuentaCorrienteViewModel _cuentaCorrienteViewModel;

    public MainViewModel()
    {
        _pedidosViewModel = new PedidosViewModel();
        _facturacionViewModel = new FacturacionViewModel();
        _clientesViewModel = new ClientesViewModel();
        _productosViewModel = new ProductosViewModel();
        _cuentaCorrienteViewModel = new CuentaCorrienteViewModel();

        CurrentView = _pedidosViewModel;
    }

    [RelayCommand]
    private void NavigateToPedidos()
    {
        CurrentSection = "Pedidos";
        CurrentView = _pedidosViewModel;
    }

    [RelayCommand]
    private void NavigateToFacturacion()
    {
        CurrentSection = "Facturación";
        CurrentView = _facturacionViewModel;
    }

    [RelayCommand]
    private void NavigateToClientes()
    {
        CurrentSection = "Clientes";
        CurrentView = _clientesViewModel;
    }

    [RelayCommand]
    private void NavigateToProductos()
    {
        CurrentSection = "Productos";
        CurrentView = _productosViewModel;
    }

    [RelayCommand]
    private void NavigateToCuentaCorriente()
    {
        CurrentSection = "Cuenta Corriente";
        CurrentView = _cuentaCorrienteViewModel;
    }
}
