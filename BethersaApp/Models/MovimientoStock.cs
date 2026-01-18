namespace BethersaApp.Models;

public class MovimientoStock
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty; // "Entrada" o "Salida"
    public string Motivo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }
}
