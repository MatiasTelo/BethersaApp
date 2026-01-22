using BethersaApp.Data;
using BethersaApp.Models;
using BethersaApp.Services;
using BethersaApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BethersaApp.ViewModels;

public partial class FacturacionViewModel : ViewModelBase
{
    private List<Pedido> _todosLosPedidosRemitados = [];
    private readonly FacturaService _facturaService;
    private int _contadorFactura = 1;

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
        _facturaService = new FacturaService();
        CargarPedidosRemitados();
        CargarContadorFactura();
    }

    private void CargarContadorFactura()
    {
        using var context = new AppDbContext();
        var ultimaFactura = context.Pedidos
            .Where(p => p.Estado == EstadoPedido.Facturado && p.NumeroFactura != null)
            .OrderByDescending(p => p.Id)
            .FirstOrDefault();

        if (ultimaFactura != null && !string.IsNullOrEmpty(ultimaFactura.NumeroFactura))
        {
            // Extraer el numero de la factura
            var partes = ultimaFactura.NumeroFactura.Split('-');
            if (partes.Length > 1 && int.TryParse(partes[1], out int numero))
            {
                _contadorFactura = numero + 1;
            }
        }
    }

    private string GenerarNumeroFactura(TipoFactura tipo)
    {
        var prefijo = tipo == TipoFactura.FacturaA ? "A" : "X";
        var numero = $"{prefijo}-{_contadorFactura:D8}";
        _contadorFactura++;
        return numero;
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
                "Validacion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Mostrar dialogo para seleccionar tipo de factura
        var dialog = new SeleccionFacturaDialog();
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        
        if (dialog.ShowDialog() != true || dialog.TipoSeleccionado == null)
        {
            return; // Cancelado
        }

        var tipoFactura = dialog.TipoSeleccionado.Value;

        using var context = new AppDbContext();
        var pedido = context.Pedidos
            .Include(p => p.Cliente)
                .ThenInclude(c => c!.Vendedor)
            .Include(p => p.Detalles)
            .FirstOrDefault(p => p.Id == PedidoSeleccionado.Id);

        if (pedido == null) return;

        // Generar numero de factura
        var numeroFactura = GenerarNumeroFactura(tipoFactura);

        // Preparar detalles para la factura
        var detallesFactura = _facturaService.PrepararDetallesFactura(pedido.Detalles.ToList(), context);

        decimal totalSinDescuento = 0;
        decimal totalConDescuento = 0;

        // Procesar cada detalle y aplicar descuentos por categoria
        foreach (var detalle in pedido.Detalles)
        {
            var producto = context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.Codigo == detalle.CodigoProducto);

            if (producto != null)
            {
                detalle.PrecioUnitario = producto.Precio;
                detalle.PorcentajeDescuento = producto.Categoria?.PorcentajeDescuento ?? 0;
                
                var descuento = detalle.PrecioUnitario * (detalle.PorcentajeDescuento / 100);
                detalle.PrecioConDescuento = detalle.PrecioUnitario - descuento;
                detalle.Subtotal = detalle.PrecioConDescuento * detalle.CantidadRecibida;

                totalSinDescuento += detalle.PrecioUnitario * detalle.CantidadRecibida;
                totalConDescuento += detalle.Subtotal;

                // Descontar stock si corresponde
                if (detalle.UsaStock && detalle.CantidadDeStock > 0)
                {
                    producto.Stock -= detalle.CantidadDeStock;

                    context.MovimientosStock.Add(new MovimientoStock
                    {
                        ProductoId = producto.Id,
                        Cantidad = detalle.CantidadDeStock,
                        TipoMovimiento = "Salida",
                        Motivo = $"Facturacion pedido {pedido.Codigo}",
                        Fecha = DateTime.Now,
                        PedidoId = pedido.Id
                    });
                }
            }
        }

        // Calcular total con IVA
        var totalConIVA = detallesFactura.Sum(d => d.ImporteFinal);

        pedido.Estado = EstadoPedido.Facturado;
        pedido.FechaFactura = DateTime.Now;
        pedido.NumeroFactura = numeroFactura;
        pedido.TipoFactura = tipoFactura == TipoFactura.FacturaA ? "A" : "X";

        // Registrar movimiento en cuenta corriente del cliente (con IVA)
        if (pedido.ClienteId > 0 && totalConIVA > 0)
        {
            var ultimoMovimiento = context.MovimientosCuentaCorriente
                .Where(m => m.ClienteId == pedido.ClienteId)
                .OrderByDescending(m => m.Id)
                .FirstOrDefault();

            var saldoAnterior = ultimoMovimiento?.Saldo ?? 0;
            var nuevoSaldo = saldoAnterior + totalConIVA;

            var movimientoCuenta = new MovimientoCuentaCorriente
            {
                TipoCuenta = TipoCuenta.Cliente,
                ClienteId = pedido.ClienteId,
                PedidoId = pedido.Id,
                Fecha = DateTime.Now,
                TipoMovimiento = TipoMovimientoCuenta.Factura,
                Descripcion = $"Factura {numeroFactura} - Pedido {pedido.Codigo}",
                NumeroComprobante = numeroFactura,
                Debe = totalConIVA,
                Haber = 0,
                Saldo = nuevoSaldo
            };

            context.MovimientosCuentaCorriente.Add(movimientoCuenta);
        }

        context.SaveChanges();

        // Generar PDF de la factura
        var datosFactura = new DatosFactura
        {
            Pedido = pedido,
            Cliente = pedido.Cliente!,
            Vendedor = pedido.Cliente?.Vendedor,
            NumeroRemito = pedido.Codigo,
            NumeroFactura = numeroFactura,
            Detalles = detallesFactura,
            TipoFactura = tipoFactura,
            FechaEmision = DateTime.Now
        };

        _facturaService.GenerarFacturaPdf(datosFactura);

        var tipoTexto = tipoFactura == TipoFactura.FacturaA ? "A" : "X (Interno)";
        System.Windows.MessageBox.Show(
            $"Factura {tipoTexto} generada exitosamente\n\n" +
            $"Numero: {numeroFactura}\n" +
            $"Cliente: {pedido.Cliente?.Nombre}\n" +
            $"Vendedor: {pedido.Cliente?.Vendedor?.Nombre ?? "Sin asignar"}\n\n" +
            $"Total con IVA: ${totalConIVA:N2}\n\n" +
            $"Se registro en cuenta corriente del cliente.",
            "Factura Generada", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

        CargarPedidosRemitados();
    }

    [RelayCommand]
    private void Refrescar()
    {
        CargarPedidosRemitados();
    }
}
