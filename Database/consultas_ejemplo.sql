-- =====================================================================
-- BethersaApp - Consultas SQL de ejemplo (SQLite)
-- Base: bin\Debug\net8.0-windows\bethersa.db  (cargar antes seed_demo.sql)
--
-- OJO con SQLite + Entity Framework: los decimales (Precio, Debe, Haber,
-- Saldo, Subtotal...) se guardan como TEXTO. Para comparar u ordenar por
-- valor hay que convertirlos: CAST(Precio AS REAL). Sin el CAST,
-- '9000' > '10000' da verdadero porque compara como texto.
--
-- Enums guardados como número:
--   Pedidos.Estado:              0 Pendiente, 1 Remitado, 2 Facturado
--   DetallesPedido.Estado:       0 Completo,  1 Parcial,  2 NoLlego
--   MovimientosCuentaCorriente.TipoCuenta:     0 Cliente, 1 Proveedor
--   MovimientosCuentaCorriente.TipoMovimiento: 0 Factura, 1 Pago,
--                               2 NotaCredito, 3 NotaDebito, 4 Ajuste
-- =====================================================================


-- ---------------------------------------------------------------------
-- 1. BÁSICAS: SELECT, WHERE, ORDER BY, LIMIT
-- ---------------------------------------------------------------------

-- 1.1 Todos los clientes activos, ordenados por nombre
SELECT Codigo, Nombre, Localidad, Provincia, TipoIva
FROM Clientes
WHERE Activo = 1
ORDER BY Nombre;

-- 1.2 Clientes de Mendoza
SELECT Codigo, Nombre, Localidad
FROM Clientes
WHERE Provincia = 'Mendoza';

-- 1.3 Buscar un cliente por parte del nombre (típico pedido de soporte)
SELECT Codigo, Nombre, Telefono, Email
FROM Clientes
WHERE Nombre LIKE '%taller%';

-- 1.4 Los 5 productos más caros
SELECT Codigo, Nombre, CAST(Precio AS REAL) AS Precio
FROM Productos
ORDER BY CAST(Precio AS REAL) DESC
LIMIT 5;

-- 1.5 Productos sin stock o con stock bajo (menos de 5 unidades)
SELECT Codigo, Nombre, Stock
FROM Productos
WHERE Activo = 1 AND Stock < 5
ORDER BY Stock;

-- 1.6 Productos con precio entre $20.000 y $50.000
SELECT Codigo, Nombre, CAST(Precio AS REAL) AS Precio
FROM Productos
WHERE CAST(Precio AS REAL) BETWEEN 20000 AND 50000
ORDER BY Precio;


-- ---------------------------------------------------------------------
-- 2. JOINS
-- ---------------------------------------------------------------------

-- 2.1 Clientes con el nombre de su vendedor (INNER JOIN: sólo los que tienen vendedor)
SELECT c.Codigo, c.Nombre AS Cliente, v.Nombre AS Vendedor
FROM Clientes c
INNER JOIN Vendedores v ON v.Id = c.VendedorId;

-- 2.2 Todos los clientes, tengan o no vendedor (LEFT JOIN)
SELECT c.Codigo, c.Nombre AS Cliente, COALESCE(v.Nombre, 'Sin asignar') AS Vendedor
FROM Clientes c
LEFT JOIN Vendedores v ON v.Id = c.VendedorId
ORDER BY Vendedor;

-- 2.3 Productos con su categoría y el % de descuento que aplica
SELECT p.Codigo, p.Nombre, cat.Nombre AS Categoria,
       CAST(cat.PorcentajeDescuento AS REAL) AS DescuentoPct
FROM Productos p
LEFT JOIN Categorias cat ON cat.Id = p.CategoriaId;

-- 2.4 Detalle completo de un pedido: cliente, productos, cantidades y subtotal
SELECT pe.Codigo AS Pedido, cl.Nombre AS Cliente, d.CodigoProducto, d.Descripcion,
       d.CantidadSolicitada, d.CantidadRecibida,
       CAST(d.PrecioConDescuento AS REAL) AS PrecioUnit,
       CAST(d.Subtotal AS REAL) AS Subtotal
FROM Pedidos pe
JOIN Clientes cl      ON cl.Id = pe.ClienteId
JOIN DetallesPedido d ON d.PedidoId = pe.Id
WHERE pe.Codigo = 'P-2026-0001';

-- 2.5 Cadena de 3 tablas: pedido -> cliente -> vendedor
SELECT pe.Codigo, pe.FechaCreacion, cl.Nombre AS Cliente, v.Nombre AS Vendedor
FROM Pedidos pe
JOIN Clientes cl        ON cl.Id = pe.ClienteId
LEFT JOIN Vendedores v  ON v.Id = cl.VendedorId
ORDER BY pe.FechaCreacion DESC;


-- ---------------------------------------------------------------------
-- 3. AGREGACIONES: COUNT, SUM, AVG, GROUP BY, HAVING
-- ---------------------------------------------------------------------

-- 3.1 Cantidad de clientes por provincia
SELECT Provincia, COUNT(*) AS Clientes
FROM Clientes
GROUP BY Provincia
ORDER BY Clientes DESC;

-- 3.2 Pedidos por estado (traduciendo el número a texto)
SELECT CASE Estado WHEN 0 THEN 'Pendiente' WHEN 1 THEN 'Remitado' WHEN 2 THEN 'Facturado' END AS Estado,
       COUNT(*) AS Pedidos
FROM Pedidos
GROUP BY Estado;

-- 3.3 Cantidad de pedidos y total facturado (neto, sin IVA) por cliente
SELECT cl.Nombre AS Cliente, COUNT(DISTINCT pe.Id) AS Pedidos,
       ROUND(SUM(CAST(d.Subtotal AS REAL)), 2) AS TotalNeto
FROM Pedidos pe
JOIN Clientes cl      ON cl.Id = pe.ClienteId
JOIN DetallesPedido d ON d.PedidoId = pe.Id
WHERE pe.Estado = 2
GROUP BY cl.Id
ORDER BY TotalNeto DESC;

-- 3.4 Clientes con más de 3 pedidos (HAVING filtra después de agrupar)
SELECT cl.Nombre, COUNT(*) AS Pedidos
FROM Pedidos pe
JOIN Clientes cl ON cl.Id = pe.ClienteId
GROUP BY cl.Id
HAVING COUNT(*) > 3
ORDER BY Pedidos DESC;

-- 3.5 Top 5 productos más vendidos (unidades recibidas en pedidos facturados)
SELECT d.CodigoProducto, d.Descripcion, SUM(d.CantidadRecibida) AS Unidades
FROM DetallesPedido d
JOIN Pedidos pe ON pe.Id = d.PedidoId
WHERE pe.Estado = 2
GROUP BY d.CodigoProducto
ORDER BY Unidades DESC
LIMIT 5;

-- 3.6 Ventas por vendedor (neto)
SELECT COALESCE(v.Nombre, 'Sin vendedor') AS Vendedor,
       COUNT(DISTINCT pe.Id) AS Pedidos,
       ROUND(SUM(CAST(d.Subtotal AS REAL)), 2) AS TotalNeto
FROM Pedidos pe
JOIN Clientes cl       ON cl.Id = pe.ClienteId
LEFT JOIN Vendedores v ON v.Id = cl.VendedorId
JOIN DetallesPedido d  ON d.PedidoId = pe.Id
WHERE pe.Estado = 2
GROUP BY v.Id
ORDER BY TotalNeto DESC;

-- 3.7 Precio promedio, mínimo y máximo por categoría
SELECT cat.Nombre AS Categoria, COUNT(*) AS Productos,
       ROUND(AVG(CAST(p.Precio AS REAL)), 2) AS Promedio,
       MIN(CAST(p.Precio AS REAL)) AS Minimo,
       MAX(CAST(p.Precio AS REAL)) AS Maximo
FROM Productos p
JOIN Categorias cat ON cat.Id = p.CategoriaId
GROUP BY cat.Id
ORDER BY Promedio DESC;


-- ---------------------------------------------------------------------
-- 4. FECHAS
-- ---------------------------------------------------------------------

-- 4.1 Facturación por mes (total con IVA, desde la cuenta corriente)
SELECT strftime('%Y-%m', Fecha) AS Mes, COUNT(*) AS Facturas,
       ROUND(SUM(CAST(Debe AS REAL)), 2) AS TotalConIVA
FROM MovimientosCuentaCorriente
WHERE TipoCuenta = 0 AND TipoMovimiento = 0
GROUP BY Mes
ORDER BY Mes;

-- 4.2 Pedidos de octubre de 2026
SELECT Codigo, FechaCreacion, Estado
FROM Pedidos
WHERE FechaCreacion >= '2026-10-01' AND FechaCreacion < '2026-11-01';

-- 4.3 Días que tardó cada pedido en facturarse
SELECT Codigo, FechaCreacion, FechaFactura,
       ROUND(julianday(FechaFactura) - julianday(FechaCreacion), 1) AS DiasHastaFactura
FROM Pedidos
WHERE Estado = 2
ORDER BY DiasHastaFactura DESC;


-- ---------------------------------------------------------------------
-- 5. SUBCONSULTAS Y "BUSCAR LO QUE FALTA"
-- ---------------------------------------------------------------------

-- 5.1 Clientes que nunca hicieron un pedido (NOT EXISTS)
SELECT Codigo, Nombre
FROM Clientes c
WHERE NOT EXISTS (SELECT 1 FROM Pedidos p WHERE p.ClienteId = c.Id);

-- 5.2 Lo mismo con LEFT JOIN ... IS NULL
SELECT c.Codigo, c.Nombre
FROM Clientes c
LEFT JOIN Pedidos p ON p.ClienteId = c.Id
WHERE p.Id IS NULL;

-- 5.3 Productos que nunca se vendieron
SELECT Codigo, Nombre, Stock
FROM Productos
WHERE Id NOT IN (SELECT ProductoId FROM DetallesPedido WHERE ProductoId IS NOT NULL);

-- 5.4 Productos más caros que el promedio
SELECT Codigo, Nombre, CAST(Precio AS REAL) AS Precio
FROM Productos
WHERE CAST(Precio AS REAL) > (SELECT AVG(CAST(Precio AS REAL)) FROM Productos)
ORDER BY Precio DESC;

-- 5.5 Productos sin categoría asignada
SELECT Codigo, Nombre FROM Productos WHERE CategoriaId IS NULL;


-- ---------------------------------------------------------------------
-- 6. CASOS DE MESA DE AYUDA
-- ---------------------------------------------------------------------

-- 6.1 "¿Cuánto me debe cada cliente?": saldo actual = último movimiento de su cuenta
SELECT cl.Codigo, cl.Nombre, CAST(m.Saldo AS REAL) AS SaldoActual, m.Fecha AS UltimoMovimiento
FROM MovimientosCuentaCorriente m
JOIN Clientes cl ON cl.Id = m.ClienteId
WHERE m.Id = (SELECT MAX(Id) FROM MovimientosCuentaCorriente WHERE ClienteId = m.ClienteId)
ORDER BY SaldoActual DESC;

-- 6.2 "El cliente dice que su saldo está mal": se recalcula desde cero y se compara
SELECT cl.Nombre,
       ROUND(SUM(CAST(m.Debe AS REAL) - CAST(m.Haber AS REAL)), 2) AS SaldoRecalculado,
       (SELECT CAST(Saldo AS REAL) FROM MovimientosCuentaCorriente
         WHERE ClienteId = cl.Id ORDER BY Id DESC LIMIT 1) AS SaldoGuardado
FROM MovimientosCuentaCorriente m
JOIN Clientes cl ON cl.Id = m.ClienteId
GROUP BY cl.Id;

-- 6.3 Resumen de cuenta corriente de un cliente (extracto)
SELECT m.Fecha,
       CASE m.TipoMovimiento WHEN 0 THEN 'Factura' WHEN 1 THEN 'Pago' WHEN 2 THEN 'Nota de crédito'
                             WHEN 3 THEN 'Nota de débito' ELSE 'Ajuste' END AS Tipo,
       m.NumeroComprobante, m.Descripcion,
       CAST(m.Debe AS REAL) AS Debe, CAST(m.Haber AS REAL) AS Haber, CAST(m.Saldo AS REAL) AS Saldo
FROM MovimientosCuentaCorriente m
JOIN Clientes cl ON cl.Id = m.ClienteId
WHERE cl.Codigo = 'CLI020'
ORDER BY m.Id;

-- 6.4 "¿Por qué este producto tiene ese stock?": historial de movimientos
SELECT m.Fecha, m.TipoMovimiento, m.Cantidad, m.Motivo
FROM MovimientosStock m
JOIN Productos p ON p.Id = m.ProductoId
WHERE p.Codigo = 'FER-PA-3001'
ORDER BY m.Fecha;

-- 6.5 Control: stock guardado vs. stock calculado por movimientos (debería dar vacío)
SELECT p.Codigo, p.Stock AS StockGuardado,
       SUM(CASE m.TipoMovimiento WHEN 'Entrada' THEN m.Cantidad ELSE -m.Cantidad END) AS StockCalculado
FROM Productos p
LEFT JOIN MovimientosStock m ON m.ProductoId = p.Id
GROUP BY p.Id
HAVING p.Stock <> COALESCE(StockCalculado, 0);

-- 6.6 Pedidos trabados: remitados hace más de 30 días y todavía sin facturar
SELECT pe.Codigo, cl.Nombre, pe.FechaRemito,
       CAST(julianday('now') - julianday(pe.FechaRemito) AS INTEGER) AS DiasSinFacturar
FROM Pedidos pe
JOIN Clientes cl ON cl.Id = pe.ClienteId
WHERE pe.Estado = 1 AND pe.FechaRemito < date('now', '-30 days');

-- 6.7 Ítems que no llegaron o llegaron incompletos
SELECT pe.Codigo, d.CodigoProducto, d.Descripcion, d.CantidadSolicitada, d.CantidadRecibida,
       CASE d.Estado WHEN 0 THEN 'Completo' WHEN 1 THEN 'Parcial' ELSE 'No llegó' END AS Estado
FROM DetallesPedido d
JOIN Pedidos pe ON pe.Id = d.PedidoId
WHERE d.Estado IN (1, 2) AND pe.Estado <> 0;

-- 6.8 Buscar datos duplicados (ej. dos clientes con el mismo CUIT)
SELECT Cuit, COUNT(*) AS Veces, GROUP_CONCAT(Nombre, ' | ') AS Clientes
FROM Clientes
GROUP BY Cuit
HAVING COUNT(*) > 1;

-- 6.9 Clientes con datos incompletos (sin email o sin teléfono)
SELECT Codigo, Nombre, Email, Telefono
FROM Clientes
WHERE Email = '' OR Email IS NULL OR Telefono = '' OR Telefono IS NULL;

-- 6.10 Buscar una factura por número
SELECT pe.Codigo, pe.NumeroFactura, pe.TipoFactura, pe.FechaFactura, cl.Nombre
FROM Pedidos pe
JOIN Clientes cl ON cl.Id = pe.ClienteId
WHERE pe.NumeroFactura = 'A-00000001';


-- ---------------------------------------------------------------------
-- 7. MODIFICAR DATOS (siempre dentro de una transacción y probando antes con SELECT)
-- ---------------------------------------------------------------------

-- 7.1 Corregir el email de un cliente
BEGIN TRANSACTION;
SELECT Codigo, Nombre, Email FROM Clientes WHERE Codigo = 'CLI005';   -- 1) ver qué se va a tocar
UPDATE Clientes SET Email = 'juan.perez@gmail.com' WHERE Codigo = 'CLI005';
SELECT Codigo, Nombre, Email FROM Clientes WHERE Codigo = 'CLI005';   -- 2) verificar
ROLLBACK;   -- cambiar por COMMIT para dejarlo guardado

-- 7.2 Dar de baja lógica un producto (no se borra, se desactiva)
BEGIN TRANSACTION;
UPDATE Productos SET Activo = 0 WHERE Codigo = 'COR-EX-6102';
ROLLBACK;

-- 7.3 Aumento del 10% a todos los productos de un proveedor
BEGIN TRANSACTION;
UPDATE Productos
SET Precio = CAST(ROUND(CAST(Precio AS REAL) * 1.10, 2) AS TEXT)
WHERE Proveedor = 'CORVEN S.A.';
SELECT Codigo, Nombre, Precio FROM Productos WHERE Proveedor = 'CORVEN S.A.';
ROLLBACK;

-- 7.4 Insertar un cliente nuevo
BEGIN TRANSACTION;
INSERT INTO Clientes (Codigo, Nombre, Direccion, Localidad, Provincia, CodigoPostal,
                      Telefono, Email, TipoIva, Cuit, Activo, VendedorId)
VALUES ('CLI099', 'Cliente de Prueba', 'Calle Falsa 123', 'Godoy Cruz', 'Mendoza', '5501',
        '261-4000000', 'prueba@mail.com', 'Consumidor Final', '20-12345678-9', 1,
        (SELECT Id FROM Vendedores WHERE Codigo = 'VEN004'));
SELECT * FROM Clientes WHERE Codigo = 'CLI099';
ROLLBACK;


-- ---------------------------------------------------------------------
-- 8. ESTRUCTURA DE LA BASE
-- ---------------------------------------------------------------------

-- 8.1 Listar tablas
SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;

-- 8.2 Columnas de una tabla
PRAGMA table_info(Pedidos);

-- 8.3 Claves foráneas de una tabla
PRAGMA foreign_key_list(DetallesPedido);

-- 8.4 Índices existentes
SELECT name, tbl_name, sql FROM sqlite_master WHERE type = 'index' AND sql IS NOT NULL;

-- 8.5 Vista con el saldo de cada cliente (se crea una vez y se consulta como una tabla)
CREATE VIEW IF NOT EXISTS vw_saldo_clientes AS
SELECT cl.Codigo, cl.Nombre, CAST(m.Saldo AS REAL) AS Saldo
FROM MovimientosCuentaCorriente m
JOIN Clientes cl ON cl.Id = m.ClienteId
WHERE m.Id = (SELECT MAX(Id) FROM MovimientosCuentaCorriente WHERE ClienteId = m.ClienteId);

SELECT * FROM vw_saldo_clientes ORDER BY Saldo DESC;
