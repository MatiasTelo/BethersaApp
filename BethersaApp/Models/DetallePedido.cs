namespace BethersaApp.Models;

public enum EstadoProducto
{
    Completo,
    Parcial,
    NoLlego
}

public class DetallePedido
{
    public int Id { get; set; }
    
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    
    public int? ProductoId { get; set; }
    public Producto? Producto { get; set; }
    
    public string CodigoProducto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CantidadSolicitada { get; set; }
    public int CantidadRecibida { get; set; }
    public EstadoProducto Estado { get; set; } = EstadoProducto.NoLlego;
    public bool UsaStock { get; set; }
    public int CantidadDeStock { get; set; }
}
