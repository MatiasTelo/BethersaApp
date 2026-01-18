using BethersaApp.Data;
using BethersaApp.Models;
using BethersaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace BethersaApp.ViewModels;

public partial class DetallePedidoViewModel : ObservableObject
{
    [ObservableProperty]
    private string _codigoProducto = string.Empty;

    [ObservableProperty]
    private string _descripcion = string.Empty;

    [ObservableProperty]
    private int _cantidadSolicitada;

    [ObservableProperty]
    private int _cantidadRecibida;

    [ObservableProperty]
    private bool _esCompleto;

    [ObservableProperty]
    private bool _esParcial;

    [ObservableProperty]
    private bool _noLlego = true;

    [ObservableProperty]
    private bool _usaStock;

    [ObservableProperty]
    private int _cantidadDeStock;

    [ObservableProperty]
    private decimal _precioUnitario;

    [ObservableProperty]
    private bool _productoValido = true;

    [ObservableProperty]
    private string _mensajeError = string.Empty;

    public decimal Subtotal => CantidadRecibida * PrecioUnitario;

    partial void OnEsCompletoChanged(bool value)
    {
        if (value)
        {
            EsParcial = false;
            NoLlego = false;
            CantidadRecibida = CantidadSolicitada;
            OnPropertyChanged(nameof(Subtotal));
        }
    }

    partial void OnEsParcialChanged(bool value)
    {
        if (value)
        {
            EsCompleto = false;
            NoLlego = false;
        }
    }

    partial void OnNoLlegoChanged(bool value)
    {
        if (value)
        {
            EsCompleto = false;
            EsParcial = false;
            CantidadRecibida = 0;
            OnPropertyChanged(nameof(Subtotal));
        }
    }

    partial void OnCantidadRecibidaChanged(int value)
    {
        OnPropertyChanged(nameof(Subtotal));
    }
}

public partial class PedidosViewModel : ViewModelBase
{
    private readonly CsvImportService _csvService;
    private readonly RemitoService _remitoService;
    private List<Pedido> _todosLosPedidos = [];

    [ObservableProperty]
    private ObservableCollection<Pedido> _pedidos = [];

    [ObservableProperty]
    private Pedido? _pedidoSeleccionado;

    [ObservableProperty]
    private ObservableCollection<DetallePedidoViewModel> _detallesActuales = [];

    [ObservableProperty]
    private string _codigoNuevoPedido = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Cliente> _clientes = [];

    [ObservableProperty]
    private Cliente? _clienteSeleccionado;

    [ObservableProperty]
    private bool _modoEdicion;

    [ObservableProperty]
    private string _erroresValidacion = string.Empty;

    private string _textoBusqueda = string.Empty;
    public string TextoBusqueda
    {
        get => _textoBusqueda;
        set
        {
            if (SetProperty(ref _textoBusqueda, value))
            {
                FiltrarPedidos();
            }
        }
    }

    private void FiltrarPedidos()
    {
        if (string.IsNullOrWhiteSpace(_textoBusqueda))
        {
            Pedidos = new ObservableCollection<Pedido>(_todosLosPedidos);
        }
        else
        {
            var busqueda = _textoBusqueda.ToLower();
            var filtrados = _todosLosPedidos
                .Where(p => p.Codigo.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase) || 
                           (p.Cliente != null && p.Cliente.Nombre.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            Pedidos = new ObservableCollection<Pedido>(filtrados);
        }
    }

    public PedidosViewModel()
    {
        _csvService = new CsvImportService();
        _remitoService = new RemitoService();
        CargarDatos();
    }

    private void CargarDatos()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        _todosLosPedidos = context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .OrderByDescending(p => p.FechaCreacion)
            .ToList();

        Pedidos = new ObservableCollection<Pedido>(_todosLosPedidos);
        Clientes = new ObservableCollection<Cliente>(context.Clientes.Where(c => c.Activo).ToList());
    }

    [RelayCommand]
    private void CargarCsv()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Archivos CSV (*.csv)|*.csv",
            Title = "Seleccionar archivo de pedido"
        };

        if (dialog.ShowDialog() == true)
        {
            var productos = _csvService.ImportarPedidoCsv(dialog.FileName);
            DetallesActuales.Clear();
            ErroresValidacion = string.Empty;

            using var context = new AppDbContext();
            var productosNoEncontrados = new List<string>();

            foreach (var producto in productos)
            {
                var productoDb = context.Productos.FirstOrDefault(p => p.Codigo == producto.Codigo);
                
                var detalle = new DetallePedidoViewModel
                {
                    CodigoProducto = producto.Codigo,
                    Descripcion = producto.Nombre,
                    CantidadSolicitada = producto.Cantidad,
                    NoLlego = true
                };

                if (productoDb != null)
                {
                    detalle.PrecioUnitario = productoDb.Precio;
                    detalle.ProductoValido = true;
                }
                else
                {
                    detalle.ProductoValido = false;
                    detalle.MensajeError = "Producto no encontrado en el sistema";
                    detalle.PrecioUnitario = producto.PrecioUnitario;
                    productosNoEncontrados.Add(producto.Codigo);
                }

                DetallesActuales.Add(detalle);
            }

            if (productosNoEncontrados.Count > 0)
            {
                ErroresValidacion = $"Productos no encontrados: {string.Join(", ", productosNoEncontrados)}";
                System.Windows.MessageBox.Show(
                    $"Los siguientes codigos de producto no existen en el sistema:\n\n{string.Join("\n", productosNoEncontrados)}\n\nSe usara el precio del CSV para estos productos.",
                    "Advertencia - Productos no encontrados",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }

            ModoEdicion = true;
        }
    }

    [RelayCommand]
    private void GuardarPedido()
    {
        if (string.IsNullOrWhiteSpace(CodigoNuevoPedido) || ClienteSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Debe ingresar un codigo de pedido y seleccionar un cliente.", 
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!DetallesActuales.Any())
        {
            System.Windows.MessageBox.Show("Debe agregar al menos un producto al pedido.", 
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();

        // Verificar que el codigo no exista
        if (context.Pedidos.Any(p => p.Codigo == CodigoNuevoPedido))
        {
            System.Windows.MessageBox.Show("Ya existe un pedido con ese codigo.", 
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        var pedido = new Pedido
        {
            Codigo = CodigoNuevoPedido,
            ClienteId = ClienteSeleccionado.Id,
            FechaCreacion = DateTime.Now,
            Estado = EstadoPedido.Pendiente
        };

        foreach (var detalle in DetallesActuales)
        {
            var estado = detalle.EsCompleto ? EstadoProducto.Completo :
                         detalle.EsParcial ? EstadoProducto.Parcial :
                         EstadoProducto.NoLlego;

            // Buscar el producto en la base de datos
            var productoDb = context.Productos.FirstOrDefault(p => p.Codigo == detalle.CodigoProducto);

            pedido.Detalles.Add(new DetallePedido
            {
                CodigoProducto = detalle.CodigoProducto,
                Descripcion = detalle.Descripcion,
                CantidadSolicitada = detalle.CantidadSolicitada,
                CantidadRecibida = detalle.CantidadRecibida,
                Estado = estado,
                UsaStock = detalle.UsaStock,
                CantidadDeStock = detalle.CantidadDeStock,
                ProductoId = productoDb?.Id
            });
        }

        context.Pedidos.Add(pedido);
        context.SaveChanges();

        System.Windows.MessageBox.Show($"Pedido {CodigoNuevoPedido} guardado correctamente.", 
            "Pedido Guardado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

        CargarDatos();
        LimpiarFormulario();
    }

    [RelayCommand]
    private void GenerarRemito()
    {
        if (PedidoSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Debe seleccionar un pedido.", 
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();
        var pedido = context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .FirstOrDefault(p => p.Id == PedidoSeleccionado.Id);

        if (pedido == null) return;

        // Preparar los detalles para el remito (solo los que llegaron)
        var detallesRemito = new List<DetalleRemitoDto>();
        
        foreach (var detalle in pedido.Detalles.Where(d => d.Estado != EstadoProducto.NoLlego && d.CantidadRecibida > 0))
        {
            var producto = context.Productos.FirstOrDefault(p => p.Codigo == detalle.CodigoProducto);
            detallesRemito.Add(new DetalleRemitoDto
            {
                CodigoProducto = detalle.CodigoProducto,
                Descripcion = detalle.Descripcion,
                Cantidad = detalle.CantidadRecibida,
                PrecioUnitario = producto?.Precio ?? 0
            });
        }

        if (!detallesRemito.Any())
        {
            System.Windows.MessageBox.Show("No hay productos recibidos para generar el remito.", 
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Generar el PDF
        try
        {
            _remitoService.GenerarRemitoPdf(pedido, detallesRemito);

            // Actualizar estado del pedido
            pedido.Estado = EstadoPedido.Remitado;
            pedido.FechaRemito = DateTime.Now;
            context.SaveChanges();

            CargarDatos();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar el remito: {ex.Message}", 
                "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void VerDetallePedido()
    {
        if (PedidoSeleccionado == null) return;


        using var context = new AppDbContext();
        var detalles = context.DetallesPedido
            .Where(d => d.PedidoId == PedidoSeleccionado.Id)
            .ToList();

        DetallesActuales.Clear();
        foreach (var detalle in detalles)
        {
            var producto = context.Productos.FirstOrDefault(p => p.Codigo == detalle.CodigoProducto);
            
            DetallesActuales.Add(new DetallePedidoViewModel
            {
                CodigoProducto = detalle.CodigoProducto,
                Descripcion = detalle.Descripcion,
                CantidadSolicitada = detalle.CantidadSolicitada,
                CantidadRecibida = detalle.CantidadRecibida,
                EsCompleto = detalle.Estado == EstadoProducto.Completo,
                EsParcial = detalle.Estado == EstadoProducto.Parcial,
                NoLlego = detalle.Estado == EstadoProducto.NoLlego,
                UsaStock = detalle.UsaStock,
                CantidadDeStock = detalle.CantidadDeStock,
                PrecioUnitario = producto?.Precio ?? 0,
                ProductoValido = producto != null
            });
        }
    }

    [RelayCommand]
    private void NuevoPedido()
    {
        LimpiarFormulario();
        ModoEdicion = true;
    }

    [RelayCommand]
    private void Refrescar()
    {
        CargarDatos();
    }

    private void LimpiarFormulario()
    {
        DetallesActuales.Clear();
        CodigoNuevoPedido = string.Empty;
        ClienteSeleccionado = null;
        ModoEdicion = false;
        ErroresValidacion = string.Empty;
    }
}
