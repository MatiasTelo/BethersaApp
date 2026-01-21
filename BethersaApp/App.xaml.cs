using BethersaApp.Data;
using BethersaApp.Models;
using System.Windows;

namespace BethersaApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        // Agregar vendedores de prueba si no existen
        if (!context.Vendedores.Any())
        {
            var vendedores = new List<Vendedor>
            {
                new() { Codigo = "VEN001", Nombre = "Carlos Martinez", Direccion = "Av. Lavalle 456", Localidad = "Resistencia", Provincia = "Chaco", CodigoPostal = "3500", Telefono = "362-4111222", Cuit = "20-11111111-1", Activo = true },
                new() { Codigo = "VEN002", Nombre = "Maria Gonzalez", Direccion = "Calle Belgrano 789", Localidad = "Corrientes", Provincia = "Corrientes", CodigoPostal = "3400", Telefono = "379-4333444", Cuit = "27-22222222-2", Activo = true },
                new() { Codigo = "VEN003", Nombre = "Roberto Sanchez", Direccion = "Av. San Martin 321", Localidad = "Formosa", Provincia = "Formosa", CodigoPostal = "3600", Telefono = "370-4555666", Cuit = "20-33333333-3", Activo = true }
            };
            context.Vendedores.AddRange(vendedores);
            context.SaveChanges();
        }

        // Agregar datos de prueba de clientes si no existen
        if (!context.Clientes.Any())
        {
            var vendedor1 = context.Vendedores.FirstOrDefault(v => v.Codigo == "VEN001");
            var vendedor2 = context.Vendedores.FirstOrDefault(v => v.Codigo == "VEN002");
            
            var clientes = new List<Cliente>
            {
                new() { Codigo = "CLI001", Nombre = "Repuestos del Norte SRL", Direccion = "Av. San Martin 1234", Localidad = "Resistencia", Provincia = "Chaco", CodigoPostal = "3500", Telefono = "362-4123456", Email = "contacto@repuestosnorte.com", TipoIva = "Responsable Inscripto", Cuit = "30-12345678-9", VendedorId = vendedor1?.Id, Activo = true },
                new() { Codigo = "CLI002", Nombre = "AutoPartes Rodriguez", Direccion = "Calle 25 de Mayo 567", Localidad = "Corrientes", Provincia = "Corrientes", CodigoPostal = "3400", Telefono = "379-4567890", Email = "ventas@autopartesrodriguez.com", TipoIva = "Monotributista", Cuit = "20-87654321-0", VendedorId = vendedor2?.Id, Activo = true },
                new() { Codigo = "CLI003", Nombre = "Taller Mecanico Los Amigos", Direccion = "Ruta 11 Km 5", Localidad = "Formosa", Provincia = "Formosa", CodigoPostal = "3600", Telefono = "370-4112233", Email = "losamigos@gmail.com", TipoIva = "Monotributista", Cuit = "23-11223344-5", VendedorId = vendedor1?.Id, Activo = true },
                new() { Codigo = "CLI004", Nombre = "Lubricentro Express", Direccion = "Av. 9 de Julio 890", Localidad = "Posadas", Provincia = "Misiones", CodigoPostal = "3300", Telefono = "376-4998877", Email = "lubriexpress@hotmail.com", TipoIva = "Responsable Inscripto", Cuit = "30-99887766-1", VendedorId = vendedor2?.Id, Activo = true },
                new() { Codigo = "CLI005", Nombre = "Juan Perez", Direccion = "Barrio Centro", Localidad = "Resistencia", Provincia = "Chaco", CodigoPostal = "3500", Telefono = "362-155667788", Email = "juanperez@gmail.com", TipoIva = "Consumidor Final", Cuit = "20-33445566-7", Activo = true }
            };
            context.Clientes.AddRange(clientes);
        }

        // Agregar categorias con descuentos si no existen
        if (!context.Categorias.Any())
        {
            var categorias = new List<Categoria>
            {
                // Categorias originales
                new() { Nombre = "Amort CORVEN", PorcentajeDescuento = 42.78m },
                new() { Nombre = "Amort LIP", PorcentajeDescuento = 42.78m },
                new() { Nombre = "Amort SADAR", PorcentajeDescuento = 19.41m },
                new() { Nombre = "Amort SUPER PICKUP", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Amort PRO TUNNING", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Pastillas FERODO", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Pastillas JURID", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Embragues VALEO", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Embragues SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Embragues CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Pastillas CORVEN HT", PorcentajeDescuento = 39.12m },
                new() { Nombre = "FTE", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Discos y Camp CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Discos y Camp HF", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Pastillas CORVEN C", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Pulmon frenos SADAR", PorcentajeDescuento = 32.55m },
                new() { Nombre = "Brazos Susp SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Parrillas SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Rotulas CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Rotulas SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Parrillas CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Brazos Susp CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Axiales CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Bieletas CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Cazoletas SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Extremos CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Susp Neumatica SADAR", PorcentajeDescuento = 35.59m },
                new() { Nombre = "Axiales SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Extremos SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "CTR", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Cazoletas CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Bieletas SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Cajas Mec CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Barras HD SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Bombas Hid CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Cajas Hid CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Gas Spring Stabilus", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Homocineticas CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Rodamientos CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Semiejes CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Mazas HF", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Homocineticas SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Rodamientos SADAR", PorcentajeDescuento = 0.00m },
                new() { Nombre = "Mazas CORVEN", PorcentajeDescuento = 39.12m },
                new() { Nombre = "Soporte Motor CORVEN", PorcentajeDescuento = 39.12m },
                
                // Categorías con 33.5%
                new() { Nombre = "Parrilla suspension", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Cremallera", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Amortiguadores YK", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Extremos YK", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Bomba hidraulica", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Bieleta", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Axialprecap", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Rotula", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Homocinetica", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Semieje", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Maza de Rueda", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Deposito de Agua", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Bomba combustible", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Electroventilador", PorcentajeDescuento = 33.5m },
                new() { Nombre = "CILINDRO RUEDA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "PISTON CILINDRO RUEDA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "CUBETA CILINDRO RUEDA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "GUARDAPOLVO CILINDRO RUEDA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "BOMBA FRENO", PorcentajeDescuento = 33.5m },
                new() { Nombre = "CUBETA PRIMARIA BOMBA FRENO", PorcentajeDescuento = 33.5m },
                new() { Nombre = "CUBETA SECUNDARIA BOMBA FRENO", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Kit Reparacion BOMBA FRENO", PorcentajeDescuento = 33.5m },
                new() { Nombre = "BOMBA EMBRAGUE", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Kit Reparacion BOMBA EMBRAGUE", PorcentajeDescuento = 33.5m },
                new() { Nombre = "BOMBIN EMBRAGUE", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Kit Reparacion BOMBIN EMBRAGUE", PorcentajeDescuento = 33.5m },
                new() { Nombre = "PISTON MORDAZA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "KIT MORDAZA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Kit Reparacion MORDAZA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "PISTON AUTORREGULABLE", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Kit Reparacion Cilindro", PorcentajeDescuento = 33.5m },
                new() { Nombre = "Kit Reparacion Servo Freno", PorcentajeDescuento = 33.5m },
                new() { Nombre = "KIT PERNOS", PorcentajeDescuento = 33.5m },
                new() { Nombre = "KIT BUJES", PorcentajeDescuento = 33.5m },
                new() { Nombre = "FLEXIBLE", PorcentajeDescuento = 33.5m },
                new() { Nombre = "VALVULA LIMITADORA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "VALVULA RETENCION", PorcentajeDescuento = 33.5m },
                new() { Nombre = "VALVULA EQUALIZADORA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "VALVULA CONTROL", PorcentajeDescuento = 33.5m },
                new() { Nombre = "VALVULA VACIO", PorcentajeDescuento = 33.5m },
                new() { Nombre = "ASIENTO VALVULA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "CUERPO VALVULA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "PURGADORES", PorcentajeDescuento = 33.5m },
                new() { Nombre = "DEPOSITO LIQUIDO", PorcentajeDescuento = 33.5m },
                new() { Nombre = "BOMBA VACIO", PorcentajeDescuento = 33.5m },
                new() { Nombre = "GUARDAPOLVO RUEDA", PorcentajeDescuento = 33.5m },
                new() { Nombre = "GUARDAPOLVO BOMBIN", PorcentajeDescuento = 33.5m },
                new() { Nombre = "GUARDAPOLVO FUELLE", PorcentajeDescuento = 33.5m },
                new() { Nombre = "SELLOS", PorcentajeDescuento = 33.5m },
                new() { Nombre = "VARIOS", PorcentajeDescuento = 33.5m },
                new() { Nombre = "PISTON SERVOFRENO", PorcentajeDescuento = 33.5m },
                
                // Categoría sin descuento para productos sin categoría asignada
                new() { Nombre = "Sin Categoria", PorcentajeDescuento = 0.00m }
            };
            context.Categorias.AddRange(categorias);
        }

        context.SaveChanges();
    }
}

