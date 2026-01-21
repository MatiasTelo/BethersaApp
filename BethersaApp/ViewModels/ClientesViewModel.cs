using BethersaApp.Data;
using BethersaApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BethersaApp.ViewModels;

public partial class ClientesViewModel : ViewModelBase
{
    private List<Cliente> _todosLosClientes = [];
    private List<Vendedor> _todosLosVendedores = [];

    [ObservableProperty]
    private ObservableCollection<Cliente> _clientes = [];

    [ObservableProperty]
    private Cliente? _clienteSeleccionado;

    [ObservableProperty]
    private bool _modoEdicion;

    [ObservableProperty]
    private string _codigo = string.Empty;

    [ObservableProperty]
    private string _nombre = string.Empty;

    [ObservableProperty]
    private string _direccion = string.Empty;

    [ObservableProperty]
    private string _localidad = string.Empty;

    [ObservableProperty]
    private string _provincia = string.Empty;

    [ObservableProperty]
    private string _codigoPostal = string.Empty;

    [ObservableProperty]
    private string _telefono = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _tipoIva = string.Empty;

    [ObservableProperty]
    private string _cuit = string.Empty;

    [ObservableProperty]
    private bool _esNuevo;

    // Propiedades para Vendedores
    [ObservableProperty]
    private ObservableCollection<Vendedor> _vendedores = [];

    [ObservableProperty]
    private Vendedor? _vendedorSeleccionadoCliente;

    [ObservableProperty]
    private Vendedor? _vendedorSeleccionado;

    [ObservableProperty]
    private string _codigoVendedor = string.Empty;

    [ObservableProperty]
    private string _nombreVendedor = string.Empty;

    [ObservableProperty]
    private string _direccionVendedor = string.Empty;

    [ObservableProperty]
    private string _localidadVendedor = string.Empty;

    [ObservableProperty]
    private string _provinciaVendedor = string.Empty;

    [ObservableProperty]
    private string _codigoPostalVendedor = string.Empty;

    [ObservableProperty]
    private string _telefonoVendedor = string.Empty;

    [ObservableProperty]
    private string _cuitVendedor = string.Empty;

    [ObservableProperty]
    private bool _modoEdicionVendedor;

    [ObservableProperty]
    private bool _esNuevoVendedor;

    private string _textoBusqueda = string.Empty;
    public string TextoBusqueda
    {
        get => _textoBusqueda;
        set
        {
            if (SetProperty(ref _textoBusqueda, value))
            {
                FiltrarClientes();
            }
        }
    }

    private string _textoBusquedaVendedor = string.Empty;
    public string TextoBusquedaVendedor
    {
        get => _textoBusquedaVendedor;
        set
        {
            if (SetProperty(ref _textoBusquedaVendedor, value))
            {
                FiltrarVendedores();
            }
        }
    }

    private void FiltrarClientes()
    {
        if (string.IsNullOrWhiteSpace(_textoBusqueda))
        {
            Clientes = new ObservableCollection<Cliente>(_todosLosClientes);
        }
        else
        {
            var busqueda = _textoBusqueda.ToLower();
            var filtrados = _todosLosClientes
                .Where(c => c.Codigo.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase) || 
                           c.Nombre.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Clientes = new ObservableCollection<Cliente>(filtrados);
        }
    }

    private void FiltrarVendedores()
    {
        if (string.IsNullOrWhiteSpace(_textoBusquedaVendedor))
        {
            Vendedores = new ObservableCollection<Vendedor>(_todosLosVendedores);
        }
        else
        {
            var busqueda = _textoBusquedaVendedor.ToLower();
            var filtrados = _todosLosVendedores
                .Where(v => v.Codigo.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase) || 
                           v.Nombre.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Vendedores = new ObservableCollection<Vendedor>(filtrados);
        }
    }

    public List<string> TiposIva { get; } =
    [
        "Responsable Inscripto",
        "Monotributista",
        "Exento",
        "Consumidor Final"
    ];

    public ClientesViewModel()
    {
        CargarDatos();
    }

    private void CargarDatos()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        _todosLosClientes = context.Clientes
            .Include(c => c.Vendedor)
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToList();

        _todosLosVendedores = context.Vendedores
            .Where(v => v.Activo)
            .OrderBy(v => v.Nombre)
            .ToList();

        Clientes = new ObservableCollection<Cliente>(_todosLosClientes);
        Vendedores = new ObservableCollection<Vendedor>(_todosLosVendedores);
    }

    // ==================== GESTION DE CLIENTES ====================

    [RelayCommand]
    private void NuevoCliente()
    {
        LimpiarFormulario();
        EsNuevo = true;
        ModoEdicion = true;
    }

    [RelayCommand]
    private void EditarCliente()
    {
        if (ClienteSeleccionado == null) return;

        Codigo = ClienteSeleccionado.Codigo;
        Nombre = ClienteSeleccionado.Nombre;
        Direccion = ClienteSeleccionado.Direccion;
        Localidad = ClienteSeleccionado.Localidad;
        Provincia = ClienteSeleccionado.Provincia;
        CodigoPostal = ClienteSeleccionado.CodigoPostal;
        Telefono = ClienteSeleccionado.Telefono;
        Email = ClienteSeleccionado.Email;
        TipoIva = ClienteSeleccionado.TipoIva;
        Cuit = ClienteSeleccionado.Cuit;
        VendedorSeleccionadoCliente = Vendedores.FirstOrDefault(v => v.Id == ClienteSeleccionado.VendedorId);

        EsNuevo = false;
        ModoEdicion = true;
    }

    [RelayCommand]
    private void GuardarCliente()
    {
        if (string.IsNullOrWhiteSpace(Codigo) || string.IsNullOrWhiteSpace(Nombre))
        {
            System.Windows.MessageBox.Show("El codigo y nombre son obligatorios.",
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();

        if (EsNuevo)
        {
            var cliente = new Cliente
            {
                Codigo = Codigo,
                Nombre = Nombre,
                Direccion = Direccion,
                Localidad = Localidad,
                Provincia = Provincia,
                CodigoPostal = CodigoPostal,
                Telefono = Telefono,
                Email = Email,
                TipoIva = TipoIva,
                Cuit = Cuit,
                VendedorId = VendedorSeleccionadoCliente?.Id,
                Activo = true
            };

            context.Clientes.Add(cliente);
        }
        else if (ClienteSeleccionado != null)
        {
            var cliente = context.Clientes.Find(ClienteSeleccionado.Id);
            if (cliente != null)
            {
                cliente.Codigo = Codigo;
                cliente.Nombre = Nombre;
                cliente.Direccion = Direccion;
                cliente.Localidad = Localidad;
                cliente.Provincia = Provincia;
                cliente.CodigoPostal = CodigoPostal;
                cliente.Telefono = Telefono;
                cliente.Email = Email;
                cliente.TipoIva = TipoIva;
                cliente.Cuit = Cuit;
                cliente.VendedorId = VendedorSeleccionadoCliente?.Id;
            }
        }

        context.SaveChanges();
        CargarDatos();
        LimpiarFormulario();
    }

    [RelayCommand]
    private void EliminarCliente()
    {
        if (ClienteSeleccionado == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Esta seguro de eliminar al cliente {ClienteSeleccionado.Nombre}?",
            "Confirmar eliminacion",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            using var context = new AppDbContext();
            var cliente = context.Clientes.Find(ClienteSeleccionado.Id);
            if (cliente != null)
            {
                cliente.Activo = false;
                context.SaveChanges();
            }

            CargarDatos();
        }
    }

    [RelayCommand]
    private void Cancelar()
    {
        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        Codigo = string.Empty;
        Nombre = string.Empty;
        Direccion = string.Empty;
        Localidad = string.Empty;
        Provincia = string.Empty;
        CodigoPostal = string.Empty;
        Telefono = string.Empty;
        Email = string.Empty;
        TipoIva = string.Empty;
        Cuit = string.Empty;
        VendedorSeleccionadoCliente = null;
        ModoEdicion = false;
        EsNuevo = false;
        ClienteSeleccionado = null;
    }

    // ==================== GESTION DE VENDEDORES ====================

    [RelayCommand]
    private void NuevoVendedor()
    {
        LimpiarFormularioVendedor();
        EsNuevoVendedor = true;
        ModoEdicionVendedor = true;
    }

    [RelayCommand]
    private void EditarVendedor()
    {
        if (VendedorSeleccionado == null) return;

        CodigoVendedor = VendedorSeleccionado.Codigo;
        NombreVendedor = VendedorSeleccionado.Nombre;
        DireccionVendedor = VendedorSeleccionado.Direccion;
        LocalidadVendedor = VendedorSeleccionado.Localidad;
        ProvinciaVendedor = VendedorSeleccionado.Provincia;
        CodigoPostalVendedor = VendedorSeleccionado.CodigoPostal;
        TelefonoVendedor = VendedorSeleccionado.Telefono;
        CuitVendedor = VendedorSeleccionado.Cuit;

        EsNuevoVendedor = false;
        ModoEdicionVendedor = true;
    }

    [RelayCommand]
    private void GuardarVendedor()
    {
        if (string.IsNullOrWhiteSpace(CodigoVendedor) || string.IsNullOrWhiteSpace(NombreVendedor))
        {
            System.Windows.MessageBox.Show("El codigo y nombre son obligatorios.",
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();

        if (EsNuevoVendedor)
        {
            if (context.Vendedores.Any(v => v.Codigo == CodigoVendedor))
            {
                System.Windows.MessageBox.Show("Ya existe un vendedor con ese codigo.",
                    "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var vendedor = new Vendedor
            {
                Codigo = CodigoVendedor,
                Nombre = NombreVendedor,
                Direccion = DireccionVendedor,
                Localidad = LocalidadVendedor,
                Provincia = ProvinciaVendedor,
                CodigoPostal = CodigoPostalVendedor,
                Telefono = TelefonoVendedor,
                Cuit = CuitVendedor,
                Activo = true
            };

            context.Vendedores.Add(vendedor);
        }
        else if (VendedorSeleccionado != null)
        {
            var vendedor = context.Vendedores.Find(VendedorSeleccionado.Id);
            if (vendedor != null)
            {
                vendedor.Codigo = CodigoVendedor;
                vendedor.Nombre = NombreVendedor;
                vendedor.Direccion = DireccionVendedor;
                vendedor.Localidad = LocalidadVendedor;
                vendedor.Provincia = ProvinciaVendedor;
                vendedor.CodigoPostal = CodigoPostalVendedor;
                vendedor.Telefono = TelefonoVendedor;
                vendedor.Cuit = CuitVendedor;
            }
        }

        context.SaveChanges();
        CargarDatos();
        LimpiarFormularioVendedor();
    }

    [RelayCommand]
    private void EliminarVendedor()
    {
        if (VendedorSeleccionado == null) return;

        using var context = new AppDbContext();
        
        var clientesConVendedor = context.Clientes.Count(c => c.VendedorId == VendedorSeleccionado.Id && c.Activo);
        
        if (clientesConVendedor > 0)
        {
            System.Windows.MessageBox.Show(
                $"No se puede eliminar el vendedor porque tiene {clientesConVendedor} cliente(s) asignado(s).",
                "No se puede eliminar",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Esta seguro de eliminar al vendedor {VendedorSeleccionado.Nombre}?",
            "Confirmar eliminacion",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            var vendedor = context.Vendedores.Find(VendedorSeleccionado.Id);
            if (vendedor != null)
            {
                vendedor.Activo = false;
                context.SaveChanges();
            }

            CargarDatos();
        }
    }

    [RelayCommand]
    private void CancelarVendedor()
    {
        LimpiarFormularioVendedor();
    }

    private void LimpiarFormularioVendedor()
    {
        CodigoVendedor = string.Empty;
        NombreVendedor = string.Empty;
        DireccionVendedor = string.Empty;
        LocalidadVendedor = string.Empty;
        ProvinciaVendedor = string.Empty;
        CodigoPostalVendedor = string.Empty;
        TelefonoVendedor = string.Empty;
        CuitVendedor = string.Empty;
        ModoEdicionVendedor = false;
        EsNuevoVendedor = false;
        VendedorSeleccionado = null;
    }
}
