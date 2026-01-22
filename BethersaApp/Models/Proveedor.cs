namespace BethersaApp.Models;

public class Proveedor
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Localidad { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<MovimientoCuentaCorriente> Movimientos { get; set; } = [];
}
