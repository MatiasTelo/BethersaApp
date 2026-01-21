namespace BethersaApp.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PorcentajeDescuento { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = [];
}
