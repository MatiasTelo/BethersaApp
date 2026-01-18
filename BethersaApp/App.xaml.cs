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

        // Agregar datos de prueba de clientes si no existen
        if (!context.Clientes.Any())
        {
            var clientes = new List<Cliente>
            {
                new() { Codigo = "CLI001", Nombre = "Repuestos del Norte SRL", Direccion = "Av. San Martin 1234", Localidad = "Resistencia", Provincia = "Chaco", CodigoPostal = "3500", Telefono = "362-4123456", Email = "contacto@repuestosnorte.com", TipoIva = "Responsable Inscripto", Cuit = "30-12345678-9", Activo = true },
                new() { Codigo = "CLI002", Nombre = "AutoPartes Rodriguez", Direccion = "Calle 25 de Mayo 567", Localidad = "Corrientes", Provincia = "Corrientes", CodigoPostal = "3400", Telefono = "379-4567890", Email = "ventas@autopartesrodriguez.com", TipoIva = "Monotributista", Cuit = "20-87654321-0", Activo = true },
                new() { Codigo = "CLI003", Nombre = "Taller Mecanico Los Amigos", Direccion = "Ruta 11 Km 5", Localidad = "Formosa", Provincia = "Formosa", CodigoPostal = "3600", Telefono = "370-4112233", Email = "losamigos@gmail.com", TipoIva = "Monotributista", Cuit = "23-11223344-5", Activo = true },
                new() { Codigo = "CLI004", Nombre = "Lubricentro Express", Direccion = "Av. 9 de Julio 890", Localidad = "Posadas", Provincia = "Misiones", CodigoPostal = "3300", Telefono = "376-4998877", Email = "lubriexpress@hotmail.com", TipoIva = "Responsable Inscripto", Cuit = "30-99887766-1", Activo = true },
                new() { Codigo = "CLI005", Nombre = "Juan Perez", Direccion = "Barrio Centro", Localidad = "Resistencia", Provincia = "Chaco", CodigoPostal = "3500", Telefono = "362-155667788", Email = "juanperez@gmail.com", TipoIva = "Consumidor Final", Cuit = "20-33445566-7", Activo = true }
            };
            context.Clientes.AddRange(clientes);
        }

        // Los productos se importaran desde CSV por el usuario

        context.SaveChanges();
    }
}

