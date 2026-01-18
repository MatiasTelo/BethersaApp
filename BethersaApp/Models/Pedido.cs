namespace BethersaApp.Models;

public enum EstadoPedido
{
    Pendiente,
    Remitado,
    Facturado
}

public class Pedido
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaRemito { get; set; }
    public DateTime? FechaFactura { get; set; }
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
    
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public ICollection<DetallePedido> Detalles { get; set; } = [];
}
