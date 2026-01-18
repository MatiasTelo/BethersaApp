using BethersaApp.Data;
using BethersaApp.Models;
using BethersaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace BethersaApp.ViewModels;

public partial class RankingProductoViewModel : ObservableObject
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
}

public partial class ProductosViewModel : ViewModelBase
{
    private readonly CsvImportService _csvService;
    private List<Producto> _todosLosProductos = [];

    [ObservableProperty]
    private ObservableCollection<Producto> _productos = [];

    [ObservableProperty]
    private Producto? _productoSeleccionado;

    [ObservableProperty]
    private bool _modoEdicion;

    [ObservableProperty]
    private string _codigo = string.Empty;

    [ObservableProperty]
    private string _nombre = string.Empty;

    [ObservableProperty]
    private string _categoria = string.Empty;

    [ObservableProperty]
    private string _proveedor = string.Empty;

    [ObservableProperty]
    private decimal _precio;

    [ObservableProperty]
    private bool _esNuevo;

    [ObservableProperty]
    private int _cantidadAgregarStock;

    [ObservableProperty]
    private string _motivoStock = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RankingProductoViewModel> _rankingGeneral = [];

    [ObservableProperty]
    private ObservableCollection<Cliente> _clientes = [];

    [ObservableProperty]
    private Cliente? _clienteRankingSeleccionado;

    [ObservableProperty]
    private ObservableCollection<RankingProductoViewModel> _rankingPorCliente = [];

    private string _textoBusqueda = string.Empty;
    public string TextoBusqueda
    {
        get => _textoBusqueda;
        set
        {
            if (SetProperty(ref _textoBusqueda, value))
            {
                FiltrarProductos();
            }
        }
    }

    private void FiltrarProductos()
    {
        if (string.IsNullOrWhiteSpace(_textoBusqueda))
        {
            Productos = new ObservableCollection<Producto>(_todosLosProductos);
        }
        else
        {
            var busqueda = _textoBusqueda.ToLower();
            var filtrados = _todosLosProductos
                .Where(p => p.Codigo.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase) || 
                           p.Nombre.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Productos = new ObservableCollection<Producto>(filtrados);
        }
    }

    public ProductosViewModel()
    {
        _csvService = new CsvImportService();
        CargarDatos();
    }

    private void CargarDatos()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        _todosLosProductos = context.Productos
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToList();

        Productos = new ObservableCollection<Producto>(_todosLosProductos);
        Clientes = new ObservableCollection<Cliente>(context.Clientes.Where(c => c.Activo).ToList());

        CargarRankingGeneral();
    }

    private void CargarRankingGeneral()
    {
        using var context = new AppDbContext();

        var ranking = context.DetallesPedido
            .Include(d => d.Pedido)
            .Where(d => d.Pedido.Estado == EstadoPedido.Facturado)
            .GroupBy(d => new { d.CodigoProducto, d.Descripcion })
            .Select(g => new RankingProductoViewModel
            {
                Codigo = g.Key.CodigoProducto,
                Nombre = g.Key.Descripcion,
                CantidadVendida = g.Sum(x => x.CantidadRecibida)
            })
            .OrderByDescending(r => r.CantidadVendida)
            .Take(50)
            .ToList();

        RankingGeneral = new ObservableCollection<RankingProductoViewModel>(ranking);
    }

    [RelayCommand]
    private void RefrescarRanking()
    {
        CargarRankingGeneral();
    }

    [RelayCommand]
    private void CargarRankingCliente()
    {
        if (ClienteRankingSeleccionado == null) return;

        using var context = new AppDbContext();

        var ranking = context.DetallesPedido
            .Include(d => d.Pedido)
            .Where(d => d.Pedido.Estado == EstadoPedido.Facturado &&
                        d.Pedido.ClienteId == ClienteRankingSeleccionado.Id)
            .GroupBy(d => new { d.CodigoProducto, d.Descripcion })
            .Select(g => new RankingProductoViewModel
            {
                Codigo = g.Key.CodigoProducto,
                Nombre = g.Key.Descripcion,
                CantidadVendida = g.Sum(x => x.CantidadRecibida)
            })
            .OrderByDescending(r => r.CantidadVendida)
            .Take(50)
            .ToList();

        RankingPorCliente = new ObservableCollection<RankingProductoViewModel>(ranking);
    }

    [RelayCommand]
    private void NuevoProducto()
    {
        LimpiarFormulario();
        EsNuevo = true;
        ModoEdicion = true;
    }

    [RelayCommand]
    private void EditarProducto()
    {
        if (ProductoSeleccionado == null) return;

        Codigo = ProductoSeleccionado.Codigo;
        Nombre = ProductoSeleccionado.Nombre;
        Categoria = ProductoSeleccionado.Categoria;
        Proveedor = ProductoSeleccionado.Proveedor;
        Precio = ProductoSeleccionado.Precio;

        EsNuevo = false;
        ModoEdicion = true;
    }

    [RelayCommand]
    private void GuardarProducto()
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
            if (context.Productos.Any(p => p.Codigo == Codigo))
            {
                System.Windows.MessageBox.Show("Ya existe un producto con ese codigo.",
                    "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var producto = new Producto
            {
                Codigo = Codigo,
                Nombre = Nombre,
                Categoria = Categoria,
                Proveedor = Proveedor,
                Precio = Precio,
                Stock = 0,
                Activo = true
            };

            context.Productos.Add(producto);
        }
        else if (ProductoSeleccionado != null)
        {
            var producto = context.Productos.Find(ProductoSeleccionado.Id);
            if (producto != null)
            {
                producto.Codigo = Codigo;
                producto.Nombre = Nombre;
                producto.Categoria = Categoria;
                producto.Proveedor = Proveedor;
                producto.Precio = Precio;
            }
        }

        context.SaveChanges();
        CargarDatos();
        LimpiarFormulario();
    }

    [RelayCommand]
    private void EliminarProducto()
    {
        if (ProductoSeleccionado == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Esta seguro de eliminar el producto {ProductoSeleccionado.Nombre}?",
            "Confirmar eliminacion",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            using var context = new AppDbContext();
            var producto = context.Productos.Find(ProductoSeleccionado.Id);
            if (producto != null)
            {
                producto.Activo = false;
                context.SaveChanges();
            }

            CargarDatos();
        }
    }

    [RelayCommand]
    private void AgregarStock()
    {
        if (ProductoSeleccionado == null || CantidadAgregarStock <= 0)
        {
            System.Windows.MessageBox.Show("Seleccione un producto y especifique una cantidad valida.",
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();
        var producto = context.Productos.Find(ProductoSeleccionado.Id);

        if (producto != null)
        {
            producto.Stock += CantidadAgregarStock;

            context.MovimientosStock.Add(new MovimientoStock
            {
                ProductoId = producto.Id,
                Cantidad = CantidadAgregarStock,
                TipoMovimiento = "Entrada",
                Motivo = string.IsNullOrWhiteSpace(MotivoStock) ? "Ingreso manual" : MotivoStock,
                Fecha = DateTime.Now
            });

            context.SaveChanges();
        }

        System.Windows.MessageBox.Show($"Se agregaron {CantidadAgregarStock} unidades al stock de {ProductoSeleccionado.Nombre}.",
            "Stock actualizado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

        CantidadAgregarStock = 0;
        MotivoStock = string.Empty;
        CargarDatos();
    }

    [RelayCommand]
    private void ImportarCsv()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Archivos CSV (*.csv)|*.csv",
            Title = "Seleccionar archivo de productos"
        };

        if (dialog.ShowDialog() == true)
        {
            var productosImportados = _csvService.ImportarProductosCsv(dialog.FileName);

            using var context = new AppDbContext();
            int nuevos = 0;
            int actualizados = 0;

            foreach (var item in productosImportados)
            {
                var existente = context.Productos.FirstOrDefault(p => p.Codigo == item.Codigo);

                if (existente == null)
                {
                    context.Productos.Add(new Producto
                    {
                        Codigo = item.Codigo,
                        Nombre = item.Nombre,
                        Categoria = item.Categoria,
                        Proveedor = item.Proveedor,
                        Precio = item.Precio,
                        Stock = 0,
                        Activo = true
                    });
                    nuevos++;
                }
                else
                {
                    existente.Nombre = item.Nombre;
                    existente.Categoria = item.Categoria;
                    existente.Proveedor = item.Proveedor;
                    existente.Precio = item.Precio;
                    actualizados++;
                }
            }

            context.SaveChanges();
            CargarDatos();

            System.Windows.MessageBox.Show($"Importacion completada:\n- Productos nuevos: {nuevos}\n- Productos actualizados: {actualizados}",
                "Importacion completada", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
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
        Categoria = string.Empty;
        Proveedor = string.Empty;
        Precio = 0;
        ModoEdicion = false;
        EsNuevo = false;
        ProductoSeleccionado = null;
    }
}
