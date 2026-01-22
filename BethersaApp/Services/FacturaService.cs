using BethersaApp.Models;
using BethersaApp.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace BethersaApp.Services;

public enum TipoFactura
{
    FacturaA,
    FacturaNegro
}

public class DetalleFacturaDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioSinDescuento { get; set; }
    public decimal PorcentajeBonificacion { get; set; }
    public decimal Neto { get; set; }  // (PrecioSinDescuento * Cantidad) - Descuento
    public decimal IVA { get; set; }   // 21% del Neto
    public decimal ImporteFinal { get; set; } // Neto + IVA
}

public class DatosFactura
{
    public Pedido Pedido { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public Vendedor? Vendedor { get; set; }
    public string NumeroRemito { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public List<DetalleFacturaDto> Detalles { get; set; } = [];
    public TipoFactura TipoFactura { get; set; }
    public DateTime FechaEmision { get; set; } = DateTime.Now;
    
    public decimal SubtotalNeto => Detalles.Sum(d => d.Neto);
    public decimal TotalIVA => Detalles.Sum(d => d.IVA);
    public decimal TotalFinal => Detalles.Sum(d => d.ImporteFinal);
}

public class FacturaService
{
    private readonly ConfiguracionEmpresa _empresa;
    private const decimal TASA_IVA = 21m;

    public FacturaService()
    {
        _empresa = new ConfiguracionEmpresa();
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void GenerarFacturaPdf(DatosFactura datosFactura)
    {
        var tipoTexto = datosFactura.TipoFactura == TipoFactura.FacturaA ? "A" : "X";
        var saveDialog = new SaveFileDialog
        {
            Filter = "Archivo PDF (*.pdf)|*.pdf",
            FileName = $"Factura_{tipoTexto}_{datosFactura.NumeroFactura}_{DateTime.Now:yyyyMMdd}.pdf",
            Title = "Guardar Factura"
        };

        if (saveDialog.ShowDialog() != true) return;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => CrearEncabezado(c, datosFactura));
                page.Content().Element(c => CrearContenido(c, datosFactura));
                page.Footer().Element(c => CrearPie(c, datosFactura));
            });
        });

        documento.GeneratePdf(saveDialog.FileName);

        // Abrir el PDF generado
        Process.Start(new ProcessStartInfo
        {
            FileName = saveDialog.FileName,
            UseShellExecute = true
        });
    }

    private void CrearEncabezado(IContainer container, DatosFactura datos)
    {
        container.Column(column =>
        {
            // Primera fila: Empresa | Tipo Factura | Datos Factura
            column.Item().Row(row =>
            {
                // Datos de la empresa (izquierda)
                row.RelativeItem().Border(1).Padding(10).Column(col =>
                {
                    col.Item().Text(_empresa.RazonSocial).Bold().FontSize(14);
                    col.Item().Text(_empresa.Direccion).FontSize(8);
                    col.Item().Text($"{_empresa.Localidad}, {_empresa.Provincia} - CP: {_empresa.CodigoPostal}").FontSize(8);
                    col.Item().Text($"Tel: {_empresa.Telefono}").FontSize(8);
                    col.Item().Text($"Email: {_empresa.Email}").FontSize(8);
                });

                // Letra de factura (centro)
                row.ConstantItem(80).Border(1).Column(col =>
                {
                    col.Item().Height(50).AlignCenter().AlignMiddle()
                        .Text(datos.TipoFactura == TipoFactura.FacturaA ? "A" : "X")
                        .Bold().FontSize(40);
                    
                    if (datos.TipoFactura == TipoFactura.FacturaA)
                    {
                        col.Item().Background(Colors.Grey.Lighten3).Padding(3)
                            .Text("COD. 001").FontSize(7).AlignCenter();
                    }
                    else
                    {
                        col.Item().Background(Colors.Grey.Lighten3).Padding(3)
                            .Text("NO VALIDA AFIP").FontSize(6).AlignCenter();
                    }
                });

                // Datos de la factura (derecha)
                row.RelativeItem().Border(1).Padding(10).Column(col =>
                {
                    var tipoTexto = datos.TipoFactura == TipoFactura.FacturaA ? "FACTURA A" : "FACTURA X";
                    col.Item().Text(tipoTexto).Bold().FontSize(14);
                    col.Item().Text($"N° {datos.NumeroFactura}").FontSize(11);
                    col.Item().PaddingTop(5).Text($"Fecha: {datos.FechaEmision:dd/MM/yyyy}").FontSize(9);
                    col.Item().Text($"CUIT: {_empresa.Cuit}").FontSize(9);
                    col.Item().Text($"IVA: {_empresa.CondicionIva}").FontSize(8);
                });
            });

            column.Item().Height(10);

            // Datos del cliente
            column.Item().Border(1).Padding(10).Column(col =>
            {
                col.Item().Row(clienteRow =>
                {
                    clienteRow.RelativeItem().Column(c =>
                    {
                        c.Item().Text("DATOS DEL COMPRADOR").Bold().FontSize(10);
                        c.Item().PaddingTop(5).Text($"Razon Social: {datos.Cliente.Nombre}").FontSize(9);
                        c.Item().Text($"Domicilio: {datos.Cliente.Direccion}").FontSize(9);
                        c.Item().Text($"Localidad: {datos.Cliente.Localidad}, {datos.Cliente.Provincia}").FontSize(9);
                    });
                    
                    clienteRow.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"CUIT: {datos.Cliente.Cuit}").FontSize(9);
                        c.Item().Text($"Cond. IVA: {datos.Cliente.TipoIva}").FontSize(9);
                        c.Item().Text($"Telefono: {datos.Cliente.Telefono}").FontSize(9);
                    });
                });
            });

            column.Item().Height(5);

            // Datos del vendedor y remito
            column.Item().Border(1).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("VENDEDOR ASIGNADO").Bold().FontSize(9);
                    col.Item().Text(datos.Vendedor?.Nombre ?? "Sin vendedor asignado").FontSize(9);
                    if (datos.Vendedor != null)
                    {
                        col.Item().Text($"Tel: {datos.Vendedor.Telefono}").FontSize(8);
                    }
                });
                
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("REMITO ASOCIADO").Bold().FontSize(9);
                    col.Item().Text($"N° {datos.NumeroRemito}").FontSize(9);
                    col.Item().Text($"Pedido: {datos.Pedido.Codigo}").FontSize(8);
                });
                
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("CONDICION DE VENTA").Bold().FontSize(9);
                    col.Item().Text("Cuenta Corriente").FontSize(9);
                });
            });

            column.Item().Height(10);
        });
    }

    private void CrearContenido(IContainer container, DatosFactura datos)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);   // Codigo
                    columns.RelativeColumn(2.5f); // Descripcion
                    columns.ConstantColumn(45);   // Cantidad
                    columns.ConstantColumn(70);   // Precio s/Desc
                    columns.ConstantColumn(45);   // Bonif%
                    columns.ConstantColumn(75);   // Neto
                    columns.ConstantColumn(55);   // IVA
                    columns.ConstantColumn(80);   // Importe Final
                });

                // Encabezado de tabla
                table.Header(header =>
                {
                    var headerStyle = TextStyle.Default.Bold().FontSize(8);
                    var headerBg = Colors.Grey.Darken3;

                    header.Cell().Background(headerBg).Padding(4)
                        .Text("Codigo").Style(headerStyle).FontColor(Colors.White);
                    header.Cell().Background(headerBg).Padding(4)
                        .Text("Descripcion").Style(headerStyle).FontColor(Colors.White);
                    header.Cell().Background(headerBg).Padding(4)
                        .Text("Cant.").Style(headerStyle).FontColor(Colors.White).AlignCenter();
                    header.Cell().Background(headerBg).Padding(4)
                        .Text("P. s/Desc").Style(headerStyle).FontColor(Colors.White).AlignRight();
                    header.Cell().Background(headerBg).Padding(4)
                        .Text("Bonif%").Style(headerStyle).FontColor(Colors.White).AlignCenter();
                    header.Cell().Background(headerBg).Padding(4)
                        .Text("Neto").Style(headerStyle).FontColor(Colors.White).AlignRight();
                    header.Cell().Background(headerBg).Padding(4)
                        .Text("IVA 21%").Style(headerStyle).FontColor(Colors.White).AlignRight();
                    header.Cell().Background(headerBg).Padding(4)
                        .Text("Importe").Style(headerStyle).FontColor(Colors.White).AlignRight();
                });

                // Filas de datos
                foreach (var detalle in datos.Detalles)
                {
                    var backgroundColor = datos.Detalles.IndexOf(detalle) % 2 == 0
                        ? Colors.White
                        : Colors.Grey.Lighten4;

                    table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(4).Text(detalle.Codigo).FontSize(8);
                    table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(4).Text(detalle.Descripcion).FontSize(8);
                    table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(4).Text(detalle.Cantidad.ToString()).FontSize(8).AlignCenter();
                    table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(4).Text($"${detalle.PrecioSinDescuento:N2}").FontSize(8).AlignRight();
                    table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(4).Text($"{detalle.PorcentajeBonificacion:N2}%").FontSize(8).AlignCenter();
                    table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(4).Text($"${detalle.Neto:N2}").FontSize(8).AlignRight();
                    table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(4).Text($"${detalle.IVA:N2}").FontSize(8).AlignRight();
                    table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(4).Text($"${detalle.ImporteFinal:N2}").FontSize(8).AlignRight().Bold();
                }
            });

            column.Item().Height(15);

            // Totales
            column.Item().Row(row =>
            {
                row.RelativeItem();
                
                row.ConstantItem(300).Border(1).Column(totalesCol =>
                {
                    totalesCol.Item().Background(Colors.Grey.Lighten4).Padding(8).Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal Neto:").FontSize(10);
                        r.ConstantItem(100).Text($"${datos.SubtotalNeto:N2}").FontSize(10).AlignRight();
                    });
                    
                    totalesCol.Item().Background(Colors.Grey.Lighten4).Padding(8).Row(r =>
                    {
                        r.RelativeItem().Text("IVA 21%:").FontSize(10);
                        r.ConstantItem(100).Text($"${datos.TotalIVA:N2}").FontSize(10).AlignRight();
                    });
                    
                    totalesCol.Item().Background(Colors.Grey.Darken3).Padding(10).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL:").Bold().FontSize(14).FontColor(Colors.White);
                        r.ConstantItem(120).Text($"${datos.TotalFinal:N2}").Bold().FontSize(14).FontColor(Colors.White).AlignRight();
                    });
                });
            });
        });
    }

    private void CrearPie(IContainer container, DatosFactura datos)
    {
        container.Column(column =>
        {
            if (datos.TipoFactura == TipoFactura.FacturaNegro)
            {
                column.Item().PaddingTop(10).Background(Colors.Yellow.Lighten3).Padding(10)
                    .Text("DOCUMENTO INTERNO - NO VALIDO COMO COMPROBANTE FISCAL")
                    .Bold().FontSize(10).AlignCenter();
            }
            
            column.Item().PaddingTop(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().PaddingTop(30).LineHorizontal(1);
                    col.Item().Text("Firma del Cliente").FontSize(8).AlignCenter();
                });
                row.ConstantItem(50);
                row.RelativeItem().Column(col =>
                {
                    col.Item().PaddingTop(30).LineHorizontal(1);
                    col.Item().Text("Firma y Sello").FontSize(8).AlignCenter();
                });
            });

            column.Item().PaddingTop(10).AlignCenter().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(7));
                text.Span("Documento generado el ");
                text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).Bold();
                text.Span(" - Sistema Bethersa");
            });
        });
    }

    public List<DetalleFacturaDto> PrepararDetallesFactura(List<DetallePedido> detalles, AppDbContext context)
    {
        var result = new List<DetalleFacturaDto>();

        foreach (var detalle in detalles.Where(d => d.CantidadRecibida > 0))
        {
            var producto = context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.Codigo == detalle.CodigoProducto);

            if (producto != null)
            {
                var precioSinDescuento = producto.Precio;
                var bonificacion = producto.Categoria?.PorcentajeDescuento ?? 0;
                var subtotalSinDescuento = precioSinDescuento * detalle.CantidadRecibida;
                var descuento = subtotalSinDescuento * (bonificacion / 100);
                var neto = subtotalSinDescuento - descuento;
                var iva = neto * (TASA_IVA / 100);
                var importeFinal = neto + iva;

                result.Add(new DetalleFacturaDto
                {
                    Codigo = detalle.CodigoProducto,
                    Descripcion = detalle.Descripcion,
                    Cantidad = detalle.CantidadRecibida,
                    PrecioSinDescuento = precioSinDescuento,
                    PorcentajeBonificacion = bonificacion,
                    Neto = neto,
                    IVA = iva,
                    ImporteFinal = importeFinal
                });
            }
        }

        return result;
    }
}
