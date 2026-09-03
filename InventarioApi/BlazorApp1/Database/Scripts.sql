-- Script de base de datos para InventarioPOS
-- Ejecutar en SQL Server Management Studio o sqlcmd

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InventarioPOS')
BEGIN
    CREATE DATABASE InventarioPOS;
END
GO

USE InventarioPOS;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_productos')
BEGIN
    CREATE TABLE tbl_productos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(200) NOT NULL,
        Precio DECIMAL(18,2) NOT NULL,
        Cantidad INT NOT NULL DEFAULT 0,
        Categoria NVARCHAR(50) NOT NULL DEFAULT 'General',
        CodigoBarras NVARCHAR(50) NULL,
        Vendidos INT NOT NULL DEFAULT 0,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_ventas')
BEGIN
    CREATE TABLE tbl_ventas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FechaVenta DATETIME NOT NULL DEFAULT GETDATE(),
        Total DECIMAL(18,2) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_detalle_venta')
BEGIN
    CREATE TABLE tbl_detalle_venta (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VentaId INT NOT NULL,
        ProductoId INT NOT NULL,
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(18,2) NOT NULL,
        Subtotal DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_DetalleVenta_Venta FOREIGN KEY (VentaId) REFERENCES tbl_ventas(Id),
        CONSTRAINT FK_DetalleVenta_Producto FOREIGN KEY (ProductoId) REFERENCES tbl_productos(Id)
    );
END
GO

-- Datos de ejemplo (opcional)
IF NOT EXISTS (SELECT 1 FROM tbl_productos)
BEGIN
    INSERT INTO tbl_productos (Nombre, Precio, Cantidad, Categoria, CodigoBarras, Vendidos)
    VALUES
        ('Coca-Cola 600ml', 18.50, 50, 'Bebidas', '7501234567890', 0),
        ('Sabritas Original', 22.00, 30, 'Alimentos', '7509876543210', 0),
        ('Cable USB-C', 89.99, 15, 'Electrónica', '7501112223334', 0),
        ('Agua Natural 1L', 12.00, 40, 'Bebidas', '7505556667778', 0);
END
GO
