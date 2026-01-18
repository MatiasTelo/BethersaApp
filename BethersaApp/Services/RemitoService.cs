using BethersaApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace BethersaApp.Services;

public class RemitoService
{
    private readonly ConfiguracionEmpresa _empresa;

    public RemitoService()
    {
        _empresa = new ConfiguracionEmpresa();
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void GenerarRemitoPdf(Pedido pedido, List<DetalleRemitoDto> detalles)
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "Archivo PDF (*.pdf)|*.pdf",
            FileName = $"Remito_{pedido.Codigo}_{DateTime.Now:yyyyMMdd}.pdf",
            Title = "Guardar Remito"
        };

        if (saveDialog.ShowDialog() != true) return;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => CrearEncabezado(c, pedido));
                page.Content().Element(c => CrearContenido(c, detalles));
                page.Footer().Element(CrearPie);
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

    private void CrearEncabezado(IContainer container, Pedido pedido)
    {
        container.Column(column =>
        {
            // Encabezado con datos de empresa y cliente
            column.Item().Row(row =>
            {
                // Datos de la empresa
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(_empresa.RazonSocial).Bold().FontSize(16);
                    col.Item().Text(_empresa.Direccion);
                    col.Item().Text($"{_empresa.Localidad}, {_empresa.Provincia} - CP: {_empresa.CodigoPostal}");
                    col.Item().Text($"Tel: {_empresa.Telefono}");
                    col.Item().Text($"Email: {_empresa.Email}");
                    col.Item().Text($"CUIT: {_empresa.Cuit}");
                    col.Item().Text($"IVA: {_empresa.CondicionIva}");
                });

                // Datos del remito con logo R (COD 091)
                row.ConstantItem(180).Column(remitoCol =>
                {
                    // Logo R de Remito (COD 091 AFIP)
                    remitoCol.Item().Row(logoRow =>
                    {
                        logoRow.RelativeItem();
                        logoRow.ConstantItem(60).Height(60).Border(2).Background(Colors.White)
                            .AlignCenter().AlignMiddle()
                            .Text("R").Bold().FontSize(36);
                        logoRow.RelativeItem();
                    });
                    
                    remitoCol.Item().PaddingTop(5).Text("COD. 091").FontSize(8).AlignCenter();
                    remitoCol.Item().Text("DOCUMENTO NO VALIDO COMO FACTURA").FontSize(6).AlignCenter();
                    
                    remitoCol.Item().PaddingTop(10).Border(1).Padding(10).Column(col =>
                    {
                        col.Item().Text("REMITO").Bold().FontSize(16).AlignCenter();
                        col.Item().Text($"Nº {pedido.Codigo}").FontSize(12).AlignCenter();
                        col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(10).AlignCenter();
                    });
                });
            });

            column.Item().PaddingVertical(15).LineHorizontal(1);

            // Datos del cliente
            column.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
            {
                col.Item().Text("DATOS DEL CLIENTE").Bold();
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Cliente: {pedido.Cliente.Nombre}").Bold();
                        c.Item().Text($"Direccion: {pedido.Cliente.Direccion}");
                        c.Item().Text($"{pedido.Cliente.Localidad}, {pedido.Cliente.Provincia}");
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"CUIT: {pedido.Cliente.Cuit}");
                        c.Item().Text($"IVA: {pedido.Cliente.TipoIva}");
                        c.Item().Text($"Tel: {pedido.Cliente.Telefono}");
                    });
                });
            });

            column.Item().PaddingVertical(10);
        });
    }

    private void CrearContenido(IContainer container, List<DetalleRemitoDto> detalles)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(80);  // Codigo
                columns.RelativeColumn(3);   // Descripcion
                columns.ConstantColumn(60);  // Cantidad
                columns.ConstantColumn(80);  // Precio Unit
                columns.ConstantColumn(90);  // Subtotal
            });

            // Encabezado de tabla
            table.Header(header =>
            {
                header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                    .Text("Codigo").Bold().FontColor(Colors.White);
                header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                    .Text("Descripcion").Bold().FontColor(Colors.White);
                header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                    .Text("Cant.").Bold().FontColor(Colors.White).AlignCenter();
                header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                    .Text("P. Unit.").Bold().FontColor(Colors.White).AlignRight();
                header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                    .Text("Subtotal").Bold().FontColor(Colors.White).AlignRight();
            });

            // Filas de datos
            foreach (var detalle in detalles)
            {
                var backgroundColor = detalles.IndexOf(detalle) % 2 == 0 
                    ? Colors.White 
                    : Colors.Grey.Lighten4;

                table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(detalle.CodigoProducto);
                table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(detalle.Descripcion);
                table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(detalle.Cantidad.ToString()).AlignCenter();
                table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text($"${detalle.PrecioUnitario:N2}").AlignRight();
                table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text($"${detalle.Subtotal:N2}").AlignRight();
            }

            // Fila de total
            var total = detalles.Sum(d => d.Subtotal);
            table.Cell().ColumnSpan(4).Background(Colors.Blue.Lighten5).Padding(8)
                .Text("TOTAL").Bold().AlignRight();
            table.Cell().Background(Colors.Blue.Lighten5).Padding(8)
                .Text($"${total:N2}").Bold().AlignRight();
        });
    }

    private void CrearPie(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().PaddingTop(40).LineHorizontal(1);
                    col.Item().Text("Firma del Cliente").AlignCenter();
                });
                row.ConstantItem(50);
                row.RelativeItem().Column(col =>
                {
                    col.Item().PaddingTop(40).LineHorizontal(1);
                    col.Item().Text("Firma y Sello").AlignCenter();
                });
            });

            column.Item().PaddingTop(20).AlignCenter().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(8));
                text.Span("Documento generado el ");
                text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).Bold();
                text.Span(" - Sistema Bethersa");
            });
        });
    }
}

public class DetalleRemitoDto
{
    public string CodigoProducto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
