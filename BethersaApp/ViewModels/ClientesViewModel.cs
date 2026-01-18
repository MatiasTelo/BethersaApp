using BethersaApp.Data;
using BethersaApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BethersaApp.ViewModels;

public partial class ClientesViewModel : ViewModelBase
{
    private List<Cliente> _todosLosClientes = [];

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

    public List<string> TiposIva { get; } =
    [
        "Responsable Inscripto",
        "Monotributista",
        "Exento",
        "Consumidor Final"
    ];

    public ClientesViewModel()
    {
        CargarClientes();
    }

    private void CargarClientes()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        _todosLosClientes = context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToList();

        Clientes = new ObservableCollection<Cliente>(_todosLosClientes);
    }

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

        EsNuevo = false;
        ModoEdicion = true;
    }

    [RelayCommand]
    private void GuardarCliente()
    {
        if (string.IsNullOrWhiteSpace(Codigo) || string.IsNullOrWhiteSpace(Nombre))
        {
            System.Windows.MessageBox.Show("El código y nombre son obligatorios.",
                "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
            }
        }

        context.SaveChanges();
        CargarClientes();
        LimpiarFormulario();
    }

    [RelayCommand]
    private void EliminarCliente()
    {
        if (ClienteSeleccionado == null) return;

        var result = System.Windows.MessageBox.Show(
            $"¿Está seguro de eliminar al cliente {ClienteSeleccionado.Nombre}?",
            "Confirmar eliminación",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            using var context = new AppDbContext();
            var cliente = context.Clientes.Find(ClienteSeleccionado.Id);
            if (cliente != null)
            {
                cliente.Activo = false; // Baja lógica
                context.SaveChanges();
            }

            CargarClientes();
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
        ModoEdicion = false;
        EsNuevo = false;
        ClienteSeleccionado = null;
    }
}
