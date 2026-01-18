using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;

namespace BethersaApp.Services;

public class CsvImportService
{
    public List<PedidoCsvRow> ImportarPedidoCsv(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";",
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        
        csv.Context.RegisterClassMap<PedidoCsvRowMap>();
        return csv.GetRecords<PedidoCsvRow>().ToList();
    }

    public List<ProductoCsvRow> ImportarProductosCsv(string filePath)
    {
        // Usar cultura española/argentina para que reconozca la coma como separador decimal
        var cultureInfo = new CultureInfo("es-AR");
        var config = new CsvConfiguration(cultureInfo)
        {
            HasHeaderRecord = true,
            Delimiter = ";",
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        
        csv.Context.RegisterClassMap<ProductoCsvRowMap>();
        var productos = csv.GetRecords<ProductoCsvRow>().ToList();
        
        // Redondear precios a 2 decimales
        foreach (var producto in productos)
        {
            producto.Precio = Math.Round(producto.Precio, 2);
        }
        
        return productos;
    }
}

// Clase para importar productos: codigo, nombre, categoria, proveedor, precio
public class ProductoCsvRow
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

public sealed class ProductoCsvRowMap : ClassMap<ProductoCsvRow>
{
    public ProductoCsvRowMap()
    {
        Map(m => m.Codigo).Name("codigo", "Codigo", "Código", "CodigoProducto");
        Map(m => m.Nombre).Name("nombre", "Nombre", "Descripcion", "Descripción");
        Map(m => m.Categoria).Name("categoria", "Categoria", "Categoría");
        Map(m => m.Proveedor).Name("proveedor", "Proveedor");
        Map(m => m.Precio).Name("precio", "Precio", "PrecioUnitario");
    }
}

// Clase para importar pedidos: Código, Nombre, Marca, Cantidad, Precio Unitario, Subtotal
public class PedidoCsvRow
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public sealed class PedidoCsvRowMap : ClassMap<PedidoCsvRow>
{
    public PedidoCsvRowMap()
    {
        Map(m => m.Codigo).Name("Código", "Codigo", "codigo", "Codigo de producto", "CodigoProducto");
        Map(m => m.Nombre).Name("Nombre", "nombre", "Descripcion", "Descripción");
        Map(m => m.Marca).Name("Marca", "marca");
        Map(m => m.Cantidad).Name("Cantidad", "cantidad");
        Map(m => m.PrecioUnitario).Name("Precio Unitario", "PrecioUnitario", "Precio unitario", "precio");
        Map(m => m.Subtotal).Name("Subtotal", "subtotal");
    }
}
