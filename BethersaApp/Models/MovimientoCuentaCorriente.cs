namespace BethersaApp.Models;

public enum TipoMovimientoCuenta
{
    Factura,    // Aumenta deuda (cliente nos debe / nosotros debemos a proveedor)
    Pago,       // Disminuye deuda
    NotaCredito,
    NotaDebito,
    Ajuste
}

public enum TipoCuenta
{
    Cliente,
    Proveedor
}

public class MovimientoCuentaCorriente
{
    public int Id { get; set; }
    
    public TipoCuenta TipoCuenta { get; set; }
    
    // Relacion con Cliente (si es cuenta de cliente)
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    
    // Relacion con Proveedor (si es cuenta de proveedor)
    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    
    // Relacion con Pedido (si el movimiento viene de una factura)
    public int? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }
    
    public DateTime Fecha { get; set; } = DateTime.Now;
    public TipoMovimientoCuenta TipoMovimiento { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string NumeroComprobante { get; set; } = string.Empty;
    
    public decimal Debe { get; set; }   // Aumenta la deuda
    public decimal Haber { get; set; }  // Disminuye la deuda (pagos)
    public decimal Saldo { get; set; }  // Saldo acumulado despues del movimiento
}
