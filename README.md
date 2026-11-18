# BethersaApp

Aplicación de escritorio para la gestión comercial de una distribuidora de autopartes: pedidos, remitos, facturación, clientes, productos con stock y cuentas corrientes de clientes y proveedores.

Desarrollada en **C# / WPF (.NET 8)** con patrón **MVVM** y base de datos local **SQLite** vía Entity Framework Core.

## Funcionalidades

| Módulo | Qué hace |
|---|---|
| **Pedidos** | Alta de pedidos por cliente, importación de ítems desde CSV, control de estado por ítem (completo / parcial / no llegó) y generación de **remitos en PDF**. |
| **Facturación** | Factura pedidos remitados como **Factura A** (con IVA 21% discriminado) o **Factura X** (comprobante interno). Aplica el descuento de la categoría de cada producto, genera el PDF e impacta en la cuenta corriente del cliente. |
| **Clientes** | ABM de clientes (condición frente al IVA, CUIT, datos de contacto) y de vendedores asignados. |
| **Productos** | ABM de productos y categorías con porcentaje de descuento, ingreso de stock, importación masiva desde CSV y ranking de productos más vendidos (general y por cliente). |
| **Cuenta Corriente** | Movimientos de clientes y proveedores (facturas, pagos, notas de crédito/débito, ajustes) con saldo acumulado. ABM de proveedores. |

## Stack

- .NET 8 (`net8.0-windows`) + WPF
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) — ViewModels y comandos
- [Entity Framework Core + SQLite](https://learn.microsoft.com/ef/core/providers/sqlite/) — persistencia
- [QuestPDF](https://www.questpdf.com/) — generación de remitos y facturas en PDF (licencia Community)
- [CsvHelper](https://joshclose.github.io/CsvHelper/) — importación de pedidos y productos

## Estructura

```
BethersaApp/
├── Models/        Entidades (Cliente, Pedido, Producto, Categoria, Vendedor, Proveedor, movimientos...)
├── Data/          AppDbContext (EF Core + SQLite)
├── ViewModels/    Lógica de cada pantalla (MVVM)
├── Views/         Vistas XAML
├── Services/      PDF de facturas y remitos, importación CSV
└── Converters/    Converters de WPF
Database/
├── seed_demo.sql          Datos de demostración
└── consultas_ejemplo.sql  Consultas SQL de ejemplo sobre la base
```

## Cómo ejecutarlo

Requisitos: Windows y el [SDK de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) (o Visual Studio 2022).

```bash
git clone https://github.com/MatiasTelo/BethersaApp.git
cd BethersaApp
dotnet run --project BethersaApp
```

O abrir `BethersaApp1.sln` en Visual Studio y ejecutar con F5.

Al iniciar por primera vez, la app crea la base `bethersa.db` junto al ejecutable (`bin/Debug/net8.0-windows/`) y carga datos base: vendedores, clientes y categorías con sus descuentos.

### Datos de demostración (opcional)

Para tener pedidos, productos, facturas y movimientos de ejemplo, ejecutar `Database/seed_demo.sql` sobre `bethersa.db` **con la app cerrada** (por ejemplo con [DB Browser for SQLite](https://sqlitebrowser.org/) o `sqlite3`):

```bash
sqlite3 BethersaApp/bin/Debug/net8.0-windows/bethersa.db < Database/seed_demo.sql
```

El script se puede correr varias veces: borra los datos de demo y los vuelve a cargar sin tocar los datos base.

## Importación CSV

**Productos** — columnas: `Codigo`, `Nombre`, `Categoria`, `Proveedor`, `Precio`

**Pedidos** — columnas: `Codigo`, `Nombre`, `Marca`, `Cantidad`, `Precio Unitario`, `Subtotal`

Se aceptan variantes comunes de los encabezados (con o sin tilde, mayúsculas/minúsculas, `Descripcion` en lugar de `Nombre`, etc.).

## Notas sobre la base de datos

- Los valores decimales (precios, saldos) se guardan como texto en SQLite; para comparar u ordenar en SQL usar `CAST(campo AS REAL)`.
- Los enums se guardan como enteros. Ver el encabezado de `Database/consultas_ejemplo.sql` para la referencia de valores.

## Autor

Matias Ignacio Telo
