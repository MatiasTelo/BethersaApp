using BethersaApp.Data;
using BethersaApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BethersaApp.ViewModels;

public partial class FacturacionViewModel : ViewModelBase
{
    private List<Pedido> _todosLosPedidosRemitados = [];

    [ObservableProperty]
    private ObservableCollection<Pedido> _pedidosRemitados = [];

    [ObservableProperty]
    private Pedido? _pedidoSeleccionado;

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
            PedidosRemitados = new ObservableCollection<Pedido>(_todosLosPedidosRemitados);
        }
        else
        {
            var busqueda = _textoBusqueda.ToLower();
            var filtrados = _todosLosPedidosRemitados
                .Where(p => p.Codigo.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase) || 
                           (p.Cliente != null && p.Cliente.Nombre.ToLower().Contains(busqueda, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            PedidosRemitados = new ObservableCollection<Pedido>(filtrados);
        }
    }

    public FacturacionViewModel()
    {
        CargarPedidosRemitados();
    }

    private void CargarPedidosRemitados()
    {
        using var context = new AppDbContext();
        _todosLosPedidosRemitados = context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .Where(p => p.Estado == EstadoPedido.Remitado)
            .OrderByDescending(p => p.FechaRemito)
            .ToList();

        PedidosRemitados = new ObservableCollection<Pedido>(_todosLosPedidosRemitados);
    }

    [RelayCommand]
    private void GenerarFactura()
    {
        if (PedidoSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Debe seleccionar un pedido.",
                "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();
        var pedido = context.Pedidos
            .Include(p => p.Detalles)
            .FirstOrDefault(p => p.Id == PedidoSeleccionado.Id);

        if (pedido == null) return;

        // Descontar stock para los productos que usan stock
        foreach (var detalle in pedido.Detalles.Where(d => d.UsaStock && d.CantidadDeStock > 0))
        {
            var producto = context.Productos.FirstOrDefault(p => p.Codigo == detalle.CodigoProducto);
            if (producto != null)
            {
                producto.Stock -= detalle.CantidadDeStock;

                context.MovimientosStock.Add(new MovimientoStock
                {
                    ProductoId = producto.Id,
                    Cantidad = detalle.CantidadDeStock,
                    TipoMovimiento = "Salida",
                    Motivo = $"Facturación pedido {pedido.Codigo}",
                    Fecha = DateTime.Now,
                    PedidoId = pedido.Id
                });
            }
        }

        pedido.Estado = EstadoPedido.Facturado;
        pedido.FechaFactura = DateTime.Now;
        context.SaveChanges();

        System.Windows.MessageBox.Show($"Factura generada para el pedido {pedido.Codigo}",
            "Factura Generada", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

        CargarPedidosRemitados();
    }

    [RelayCommand]
    private void Refrescar()
    {
        CargarPedidosRemitados();
    }
}
