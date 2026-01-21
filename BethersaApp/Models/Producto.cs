namespace BethersaApp.Models;

public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; } = true;

    // Relación con Categoria
    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public ICollection<DetallePedido> DetallesPedido { get; set; } = [];
}
