using BethersaApp.Data;
using BethersaApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BethersaApp.ViewModels;

public partial class ResumenCuentaViewModel : ObservableObject
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal SaldoActual { get; set; }
}

public partial class CuentaCorrienteViewModel : ViewModelBase
{
    // ==================== CLIENTES ====================
    private List<Cliente> _todosLosClientes = [];

    [ObservableProperty]
    private ObservableCollection<ResumenCuentaViewModel> _resumenClientes = [];

    [ObservableProperty]
    private ResumenCuentaViewModel? _clienteCuentaSeleccionado;

    [ObservableProperty]
    private ObservableCollection<MovimientoCuentaCorriente> _movimientosCliente = [];

    [ObservableProperty]
    private decimal _montopagoCliente;

    [ObservableProperty]
    private string _descripcionPagoCliente = string.Empty;

    [ObservableProperty]
    private string _comprobantePagoCliente = string.Empty;

    // ==================== PROVEEDORES ====================
    private List<Proveedor> _todosLosProveedores = [];

    [ObservableProperty]
    private ObservableCollection<Proveedor> _proveedores = [];

    [ObservableProperty]
    private ObservableCollection<ResumenCuentaViewModel> _resumenProveedores = [];

    [ObservableProperty]
    private ResumenCuentaViewModel? _proveedorCuentaSeleccionado;

    [ObservableProperty]
    private ObservableCollection<MovimientoCuentaCorriente> _movimientosProveedor = [];

    [ObservableProperty]
    private Proveedor? _proveedorSeleccionado;

    // Propiedades para ABM Proveedor
    [ObservableProperty]
    private string _codigoProveedor = string.Empty;

    [ObservableProperty]
    private string _nombreProveedor = string.Empty;

    [ObservableProperty]
    private string _direccionProveedor = string.Empty;

    [ObservableProperty]
    private string _localidadProveedor = string.Empty;

    [ObservableProperty]
    private string _provinciaProveedor = string.Empty;

    [ObservableProperty]
    private string _codigoPostalProveedor = string.Empty;

    [ObservableProperty]
    private string _telefonoProveedor = string.Empty;

    [ObservableProperty]
    private string _emailProveedor = string.Empty;

    [ObservableProperty]
    private string _cuitProveedor = string.Empty;

    [ObservableProperty]
    private bool _modoEdicionProveedor;

    [ObservableProperty]
    private bool _esNuevoProveedor;

    // Propiedades para movimientos de proveedor
    [ObservableProperty]
    private decimal _montoMovimientoProveedor;

    [ObservableProperty]
    private string _descripcionMovimientoProveedor = string.Empty;

    [ObservableProperty]
    private string _comprobanteMovimientoProveedor = string.Empty;

    [ObservableProperty]
    private bool _esDeudaProveedor = true; // true = Factura/Deuda, false = Pago

    private string _textoBusquedaCliente = string.Empty;
    public string TextoBusquedaCliente
    {
        get => _textoBusquedaCliente;
        set
        {
            if (SetProperty(ref _textoBusquedaCliente, value))
            {
                FiltrarClientes();
            }
        }
    }

    private string _textoBusquedaProveedor = string.Empty;
    public string TextoBusquedaProveedor
    {
        get => _textoBusquedaProveedor;
        set
        {
            if (SetProperty(ref _textoBusquedaProveedor, value))
            {
                FiltrarProveedores();
            }
        }
    }

    public CuentaCorrienteViewModel()
    {
        CargarDatos();
    }

    private void CargarDatos()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        // Cargar clientes con sus saldos
        _todosLosClientes = context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToList();

        CargarResumenClientes();

        // Cargar proveedores
        _todosLosProveedores = context.Proveedores
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToList();

        Proveedores = new ObservableCollection<Proveedor>(_todosLosProveedores);
        CargarResumenProveedores();
    }

    private void CargarResumenClientes()
    {
        using var context = new AppDbContext();
        
        var resumen = _todosLosClientes.Select(c => new ResumenCuentaViewModel
        {
            Id = c.Id,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
            SaldoActual = context.MovimientosCuentaCorriente
                .Where(m => m.ClienteId == c.Id)
                .OrderByDescending(m => m.Id)
                .Select(m => m.Saldo)
                .FirstOrDefault()
        }).ToList();

        if (string.IsNullOrWhiteSpace(_textoBusquedaCliente))
        {
            ResumenClientes = new ObservableCollection<ResumenCuentaViewModel>(resumen);
        }
        else
        {
            var busqueda = _textoBusquedaCliente.ToLower();
            var filtrados = resumen
                .Where(c => c.Codigo.ToLower().Contains(busqueda) || 
                           c.Nombre.ToLower().Contains(busqueda))
                .ToList();
            ResumenClientes = new ObservableCollection<ResumenCuentaViewModel>(filtrados);
        }
    }

    private void CargarResumenProveedores()
    {
        using var context = new AppDbContext();
        
        var resumen = _todosLosProveedores.Select(p => new ResumenCuentaViewModel
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Nombre = p.Nombre,
            SaldoActual = context.MovimientosCuentaCorriente
                .Where(m => m.ProveedorId == p.Id)
                .OrderByDescending(m => m.Id)
                .Select(m => m.Saldo)
                .FirstOrDefault()
        }).ToList();

        if (string.IsNullOrWhiteSpace(_textoBusquedaProveedor))
        {
            ResumenProveedores = new ObservableCollection<ResumenCuentaViewModel>(resumen);
        }
        else
        {
            var busqueda = _textoBusquedaProveedor.ToLower();
            var filtrados = resumen
                .Where(p => p.Codigo.ToLower().Contains(busqueda) || 
                           p.Nombre.ToLower().Contains(busqueda))
                .ToList();
            ResumenProveedores = new ObservableCollection<ResumenCuentaViewModel>(filtrados);
        }
    }

    private void FiltrarClientes()
    {
        CargarResumenClientes();
    }

    private void FiltrarProveedores()
    {
        CargarResumenProveedores();
    }

    // ==================== COMANDOS CLIENTES ====================

    partial void OnClienteCuentaSeleccionadoChanged(ResumenCuentaViewModel? value)
    {
        if (value != null)
        {
            CargarMovimientosCliente(value.Id);
        }
        else
        {
            MovimientosCliente.Clear();
        }
    }

    private void CargarMovimientosCliente(int clienteId)
    {
        using var context = new AppDbContext();
        var movimientos = context.MovimientosCuentaCorriente
            .Include(m => m.Pedido)
            .Where(m => m.ClienteId == clienteId)
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.Id)
            .ToList();

        MovimientosCliente = new ObservableCollection<MovimientoCuentaCorriente>(movimientos);
    }

    [RelayCommand]
    private void RegistrarPagoCliente()
    {
        if (ClienteCuentaSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Seleccione un cliente.",
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (MontopagoCliente <= 0)
        {
            System.Windows.MessageBox.Show("Ingrese un monto valido.",
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();

        // Obtener saldo actual
        var ultimoMovimiento = context.MovimientosCuentaCorriente
            .Where(m => m.ClienteId == ClienteCuentaSeleccionado.Id)
            .OrderByDescending(m => m.Id)
            .FirstOrDefault();

        var saldoAnterior = ultimoMovimiento?.Saldo ?? 0;
        var nuevoSaldo = saldoAnterior - MontopagoCliente;

        var movimiento = new MovimientoCuentaCorriente
        {
            TipoCuenta = TipoCuenta.Cliente,
            ClienteId = ClienteCuentaSeleccionado.Id,
            Fecha = DateTime.Now,
            TipoMovimiento = TipoMovimientoCuenta.Pago,
            Descripcion = string.IsNullOrWhiteSpace(DescripcionPagoCliente) ? "Pago recibido" : DescripcionPagoCliente,
            NumeroComprobante = ComprobantePagoCliente,
            Debe = 0,
            Haber = MontopagoCliente,
            Saldo = nuevoSaldo
        };

        context.MovimientosCuentaCorriente.Add(movimiento);
        context.SaveChanges();

        System.Windows.MessageBox.Show($"Pago de ${MontopagoCliente:N2} registrado correctamente.",
            "Pago Registrado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

        // Limpiar y recargar
        MontopagoCliente = 0;
        DescripcionPagoCliente = string.Empty;
        ComprobantePagoCliente = string.Empty;
        
        CargarMovimientosCliente(ClienteCuentaSeleccionado.Id);
        CargarResumenClientes();
        
        // Actualizar el saldo en el item seleccionado
        ClienteCuentaSeleccionado.SaldoActual = nuevoSaldo;
    }

    // ==================== COMANDOS PROVEEDORES ====================

    partial void OnProveedorCuentaSeleccionadoChanged(ResumenCuentaViewModel? value)
    {
        if (value != null)
        {
            CargarMovimientosProveedor(value.Id);
            ProveedorSeleccionado = _todosLosProveedores.FirstOrDefault(p => p.Id == value.Id);
        }
        else
        {
            MovimientosProveedor.Clear();
            ProveedorSeleccionado = null;
        }
    }

    private void CargarMovimientosProveedor(int proveedorId)
    {
        using var context = new AppDbContext();
        var movimientos = context.MovimientosCuentaCorriente
            .Where(m => m.ProveedorId == proveedorId)
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.Id)
            .ToList();

        MovimientosProveedor = new ObservableCollection<MovimientoCuentaCorriente>(movimientos);
    }

    [RelayCommand]
    private void RegistrarMovimientoProveedor()
    {
        if (ProveedorCuentaSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Seleccione un proveedor.",
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (MontoMovimientoProveedor <= 0)
        {
            System.Windows.MessageBox.Show("Ingrese un monto valido.",
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();

        // Obtener saldo actual
        var ultimoMovimiento = context.MovimientosCuentaCorriente
            .Where(m => m.ProveedorId == ProveedorCuentaSeleccionado.Id)
            .OrderByDescending(m => m.Id)
            .FirstOrDefault();

        var saldoAnterior = ultimoMovimiento?.Saldo ?? 0;
        decimal nuevoSaldo;
        decimal debe = 0;
        decimal haber = 0;
        TipoMovimientoCuenta tipo;
        string descripcionDefault;

        if (EsDeudaProveedor)
        {
            // Es una factura/deuda - aumenta lo que debemos
            debe = MontoMovimientoProveedor;
            nuevoSaldo = saldoAnterior + MontoMovimientoProveedor;
            tipo = TipoMovimientoCuenta.Factura;
            descripcionDefault = "Factura de proveedor";
        }
        else
        {
            // Es un pago - disminuye lo que debemos
            haber = MontoMovimientoProveedor;
            nuevoSaldo = saldoAnterior - MontoMovimientoProveedor;
            tipo = TipoMovimientoCuenta.Pago;
            descripcionDefault = "Pago a proveedor";
        }

        var movimiento = new MovimientoCuentaCorriente
        {
            TipoCuenta = TipoCuenta.Proveedor,
            ProveedorId = ProveedorCuentaSeleccionado.Id,
            Fecha = DateTime.Now,
            TipoMovimiento = tipo,
            Descripcion = string.IsNullOrWhiteSpace(DescripcionMovimientoProveedor) ? descripcionDefault : DescripcionMovimientoProveedor,
            NumeroComprobante = ComprobanteMovimientoProveedor,
            Debe = debe,
            Haber = haber,
            Saldo = nuevoSaldo
        };

        context.MovimientosCuentaCorriente.Add(movimiento);
        context.SaveChanges();

        var tipoTexto = EsDeudaProveedor ? "Deuda" : "Pago";
        System.Windows.MessageBox.Show($"{tipoTexto} de ${MontoMovimientoProveedor:N2} registrado correctamente.",
            "Movimiento Registrado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

        // Limpiar y recargar
        MontoMovimientoProveedor = 0;
        DescripcionMovimientoProveedor = string.Empty;
        ComprobanteMovimientoProveedor = string.Empty;
        
        CargarMovimientosProveedor(ProveedorCuentaSeleccionado.Id);
        CargarResumenProveedores();
        
        // Actualizar el saldo en el item seleccionado
        ProveedorCuentaSeleccionado.SaldoActual = nuevoSaldo;
    }

    // ==================== ABM PROVEEDORES ====================

    [RelayCommand]
    private void NuevoProveedor()
    {
        LimpiarFormularioProveedor();
        EsNuevoProveedor = true;
        ModoEdicionProveedor = true;
    }

    [RelayCommand]
    private void EditarProveedor()
    {
        if (ProveedorSeleccionado == null) return;

        CodigoProveedor = ProveedorSeleccionado.Codigo;
        NombreProveedor = ProveedorSeleccionado.Nombre;
        DireccionProveedor = ProveedorSeleccionado.Direccion;
        LocalidadProveedor = ProveedorSeleccionado.Localidad;
        ProvinciaProveedor = ProveedorSeleccionado.Provincia;
        CodigoPostalProveedor = ProveedorSeleccionado.CodigoPostal;
        TelefonoProveedor = ProveedorSeleccionado.Telefono;
        EmailProveedor = ProveedorSeleccionado.Email;
        CuitProveedor = ProveedorSeleccionado.Cuit;

        EsNuevoProveedor = false;
        ModoEdicionProveedor = true;
    }

    [RelayCommand]
    private void GuardarProveedor()
    {
        if (string.IsNullOrWhiteSpace(CodigoProveedor) || string.IsNullOrWhiteSpace(NombreProveedor))
        {
            System.Windows.MessageBox.Show("El codigo y nombre son obligatorios.",
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();

        if (EsNuevoProveedor)
        {
            if (context.Proveedores.Any(p => p.Codigo == CodigoProveedor))
            {
                System.Windows.MessageBox.Show("Ya existe un proveedor con ese codigo.",
                    "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var proveedor = new Proveedor
            {
                Codigo = CodigoProveedor,
                Nombre = NombreProveedor,
                Direccion = DireccionProveedor,
                Localidad = LocalidadProveedor,
                Provincia = ProvinciaProveedor,
                CodigoPostal = CodigoPostalProveedor,
                Telefono = TelefonoProveedor,
                Email = EmailProveedor,
                Cuit = CuitProveedor,
                Activo = true
            };

            context.Proveedores.Add(proveedor);
        }
        else if (ProveedorSeleccionado != null)
        {
            var proveedor = context.Proveedores.Find(ProveedorSeleccionado.Id);
            if (proveedor != null)
            {
                proveedor.Codigo = CodigoProveedor;
                proveedor.Nombre = NombreProveedor;
                proveedor.Direccion = DireccionProveedor;
                proveedor.Localidad = LocalidadProveedor;
                proveedor.Provincia = ProvinciaProveedor;
                proveedor.CodigoPostal = CodigoPostalProveedor;
                proveedor.Telefono = TelefonoProveedor;
                proveedor.Email = EmailProveedor;
                proveedor.Cuit = CuitProveedor;
            }
        }

        context.SaveChanges();
        CargarDatos();
        LimpiarFormularioProveedor();
    }

    [RelayCommand]
    private void EliminarProveedor()
    {
        if (ProveedorSeleccionado == null) return;

        using var context = new AppDbContext();
        
        var tieneMovimientos = context.MovimientosCuentaCorriente.Any(m => m.ProveedorId == ProveedorSeleccionado.Id);
        
        if (tieneMovimientos)
        {
            System.Windows.MessageBox.Show(
                "No se puede eliminar el proveedor porque tiene movimientos en cuenta corriente.",
                "No se puede eliminar",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Esta seguro de eliminar al proveedor {ProveedorSeleccionado.Nombre}?",
            "Confirmar eliminacion",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            var proveedor = context.Proveedores.Find(ProveedorSeleccionado.Id);
            if (proveedor != null)
            {
                proveedor.Activo = false;
                context.SaveChanges();
            }

            CargarDatos();
        }
    }

    [RelayCommand]
    private void CancelarProveedor()
    {
        LimpiarFormularioProveedor();
    }

    private void LimpiarFormularioProveedor()
    {
        CodigoProveedor = string.Empty;
        NombreProveedor = string.Empty;
        DireccionProveedor = string.Empty;
        LocalidadProveedor = string.Empty;
        ProvinciaProveedor = string.Empty;
        CodigoPostalProveedor = string.Empty;
        TelefonoProveedor = string.Empty;
        EmailProveedor = string.Empty;
        CuitProveedor = string.Empty;
        ModoEdicionProveedor = false;
        EsNuevoProveedor = false;
    }

    [RelayCommand]
    private void Refrescar()
    {
        CargarDatos();
        if (ClienteCuentaSeleccionado != null)
        {
            CargarMovimientosCliente(ClienteCuentaSeleccionado.Id);
        }
        if (ProveedorCuentaSeleccionado != null)
        {
            CargarMovimientosProveedor(ProveedorCuentaSeleccionado.Id);
        }
    }
}
