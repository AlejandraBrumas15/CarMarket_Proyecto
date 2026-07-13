/*
    Base de datos: CarMarketDB
    Proyecto: Marketplace de compra y venta de autos usados
    Motor: Microsoft SQL Server

    Este script crea la base de datos, sus tablas, relaciones,
    procedimientos almacenados y consultas de comprobacion.
*/

USE master;
GO

IF DB_ID('CarMarketDB') IS NULL
BEGIN
    CREATE DATABASE CarMarketDB;
END;
GO

USE CarMarketDB;
GO

/* =========================================================
   1. TABLA USUARIOS
   Guarda la informacion utilizada para registrarse e iniciar sesion.
   La contrasena se almacena como un hash SHA-256, no como texto visible.
   ========================================================= */
IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        IdUsuario INT IDENTITY(1,1) NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        Edad INT NOT NULL,
        Email NVARCHAR(150) NOT NULL,
        Telefono NVARCHAR(20) NOT NULL,
        ContrasenaHash VARBINARY(32) NOT NULL,
        FechaRegistro DATETIME2(0) NOT NULL
            CONSTRAINT DF_Usuarios_FechaRegistro DEFAULT SYSDATETIME(),
        Activo BIT NOT NULL
            CONSTRAINT DF_Usuarios_Activo DEFAULT 1,

        CONSTRAINT PK_Usuarios PRIMARY KEY (IdUsuario),
        CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
        CONSTRAINT CK_Usuarios_Edad CHECK (Edad BETWEEN 16 AND 120),
        CONSTRAINT CK_Usuarios_Email CHECK (Email LIKE '%_@_%._%')
    );
END;
GO

/* =========================================================
   2. TABLA VEHICULOS
   Guarda todos los datos escritos en el formulario de venta.
   ========================================================= */
IF OBJECT_ID('dbo.Vehiculos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vehiculos
    (
        IdVehiculo INT IDENTITY(1,1) NOT NULL,
        Marca NVARCHAR(50) NOT NULL,
        Modelo NVARCHAR(80) NOT NULL,
        Anio SMALLINT NOT NULL,
        TipoVehiculo NVARCHAR(30) NOT NULL,
        PrecioOriginal DECIMAL(12,2) NOT NULL,
        PrecioVenta DECIMAL(12,2) NOT NULL,
        Kilometraje DECIMAL(12,2) NOT NULL,
        Color NVARCHAR(40) NOT NULL,
        Detalles NVARCHAR(1000) NULL,

        CONSTRAINT PK_Vehiculos PRIMARY KEY (IdVehiculo),
        CONSTRAINT CK_Vehiculos_Anio CHECK (Anio BETWEEN 1900 AND 2100),
        CONSTRAINT CK_Vehiculos_PrecioOriginal CHECK (PrecioOriginal > 0),
        CONSTRAINT CK_Vehiculos_PrecioVenta CHECK (PrecioVenta > 0),
        CONSTRAINT CK_Vehiculos_Kilometraje CHECK (Kilometraje >= 0)
    );
END;
GO

/* =========================================================
   3. TABLA PUBLICACIONES
   Relaciona al vendedor con el vehiculo publicado.
   Disponible = 1: aparece en compras.
   Disponible = 0: ya fue vendido o retirado.
   ========================================================= */
IF OBJECT_ID('dbo.Publicaciones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Publicaciones
    (
        IdPublicacion INT IDENTITY(1,1) NOT NULL,
        IdVendedor INT NOT NULL,
        IdVehiculo INT NOT NULL,
        Descripcion NVARCHAR(1000) NULL,
        FechaPublicacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_Publicaciones_Fecha DEFAULT SYSDATETIME(),
        Disponible BIT NOT NULL
            CONSTRAINT DF_Publicaciones_Disponible DEFAULT 1,

        CONSTRAINT PK_Publicaciones PRIMARY KEY (IdPublicacion),
        CONSTRAINT UQ_Publicaciones_Vehiculo UNIQUE (IdVehiculo),
        CONSTRAINT FK_Publicaciones_Usuarios FOREIGN KEY (IdVendedor)
            REFERENCES dbo.Usuarios(IdUsuario),
        CONSTRAINT FK_Publicaciones_Vehiculos FOREIGN KEY (IdVehiculo)
            REFERENCES dbo.Vehiculos(IdVehiculo)
    );
END;
GO

/* =========================================================
   4. TABLA COMPRAS
   Conserva el historial cuando un usuario compra un vehiculo.
   ========================================================= */
IF OBJECT_ID('dbo.Compras', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Compras
    (
        IdCompra INT IDENTITY(1,1) NOT NULL,
        IdPublicacion INT NOT NULL,
        IdComprador INT NOT NULL,
        PrecioPagado DECIMAL(12,2) NOT NULL,
        FechaCompra DATETIME2(0) NOT NULL
            CONSTRAINT DF_Compras_Fecha DEFAULT SYSDATETIME(),

        CONSTRAINT PK_Compras PRIMARY KEY (IdCompra),
        CONSTRAINT UQ_Compras_Publicacion UNIQUE (IdPublicacion),
        CONSTRAINT FK_Compras_Publicaciones FOREIGN KEY (IdPublicacion)
            REFERENCES dbo.Publicaciones(IdPublicacion),
        CONSTRAINT FK_Compras_Usuarios FOREIGN KEY (IdComprador)
            REFERENCES dbo.Usuarios(IdUsuario),
        CONSTRAINT CK_Compras_Precio CHECK (PrecioPagado > 0)
    );
END;
GO

/* Indices para acelerar el inicio de sesion y la busqueda de autos. */
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Usuarios_Email_Activo'
      AND object_id = OBJECT_ID('dbo.Usuarios')
)
BEGIN
    CREATE INDEX IX_Usuarios_Email_Activo
        ON dbo.Usuarios (Email, Activo);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Publicaciones_Disponible'
      AND object_id = OBJECT_ID('dbo.Publicaciones')
)
BEGIN
    CREATE INDEX IX_Publicaciones_Disponible
        ON dbo.Publicaciones (Disponible, FechaPublicacion DESC);
END;
GO

/* =========================================================
   5. PROCEDIMIENTO PARA REGISTRAR USUARIOS
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_RegistrarUsuario
    @Nombre NVARCHAR(100),
    @Edad INT,
    @Email NVARCHAR(150),
    @Telefono NVARCHAR(20),
    @Contrasena NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Nombre = LTRIM(RTRIM(@Nombre));
    SET @Email = LOWER(LTRIM(RTRIM(@Email)));
    SET @Telefono = LTRIM(RTRIM(@Telefono));

    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Email = @Email)
        THROW 50001, 'Ya existe un usuario registrado con ese correo.', 1;

    INSERT INTO dbo.Usuarios
        (Nombre, Edad, Email, Telefono, ContrasenaHash)
    VALUES
        (@Nombre, @Edad, @Email, @Telefono,
         HASHBYTES('SHA2_256', CONVERT(VARBINARY(MAX), @Contrasena)));

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS IdUsuario;
END;
GO

/* =========================================================
   6. PROCEDIMIENTO PARA INICIAR SESION
   Devuelve el usuario solamente si el correo y la contrasena coinciden.
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_IniciarSesion
    @Email NVARCHAR(150),
    @Contrasena NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdUsuario,
        Nombre,
        Edad,
        Email,
        Telefono
    FROM dbo.Usuarios
    WHERE Email = LOWER(LTRIM(RTRIM(@Email)))
      AND ContrasenaHash = HASHBYTES(
            'SHA2_256', CONVERT(VARBINARY(MAX), @Contrasena)
          )
      AND Activo = 1;
END;
GO

/* =========================================================
   7. PROCEDIMIENTO PARA PUBLICAR UN VEHICULO
   Inserta el vehiculo y su publicacion dentro de una transaccion.
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_PublicarVehiculo
    @IdVendedor INT,
    @Marca NVARCHAR(50),
    @Modelo NVARCHAR(80),
    @Anio SMALLINT,
    @TipoVehiculo NVARCHAR(30),
    @PrecioOriginal DECIMAL(12,2),
    @PrecioVenta DECIMAL(12,2),
    @Kilometraje DECIMAL(12,2),
    @Color NVARCHAR(40),
    @Detalles NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios
        WHERE IdUsuario = @IdVendedor AND Activo = 1
    )
        THROW 50002, 'El usuario vendedor no existe o esta inactivo.', 1;

    BEGIN TRANSACTION;

    INSERT INTO dbo.Vehiculos
        (Marca, Modelo, Anio, TipoVehiculo, PrecioOriginal,
         PrecioVenta, Kilometraje, Color, Detalles)
    VALUES
        (LTRIM(RTRIM(@Marca)), LTRIM(RTRIM(@Modelo)), @Anio,
         LTRIM(RTRIM(@TipoVehiculo)), @PrecioOriginal,
         @PrecioVenta, @Kilometraje, LTRIM(RTRIM(@Color)), @Detalles);

    DECLARE @IdVehiculo INT = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.Publicaciones
        (IdVendedor, IdVehiculo, Descripcion)
    VALUES
        (@IdVendedor, @IdVehiculo, @Detalles);

    DECLARE @IdPublicacion INT = CAST(SCOPE_IDENTITY() AS INT);

    COMMIT TRANSACTION;

    SELECT @IdPublicacion AS IdPublicacion,
           @IdVehiculo AS IdVehiculo;
END;
GO

/* =========================================================
   8. PROCEDIMIENTO PARA MOSTRAR VEHICULOS EN COMPRAS
   Solo devuelve publicaciones disponibles.
   Los filtros son opcionales: enviar NULL para ignorarlos.
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_ListarPublicaciones
    @Marca NVARCHAR(50) = NULL,
    @TipoVehiculo NVARCHAR(30) = NULL,
    @Anio SMALLINT = NULL,
    @PrecioMaximo DECIMAL(12,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.IdPublicacion,
        p.FechaPublicacion,
        p.Descripcion,
        u.IdUsuario AS IdVendedor,
        u.Nombre AS NombreVendedor,
        u.Telefono AS TelefonoVendedor,
        v.IdVehiculo,
        v.Marca,
        v.Modelo,
        v.Anio,
        v.TipoVehiculo,
        v.PrecioOriginal,
        v.PrecioVenta,
        v.Kilometraje,
        v.Color,
        v.Detalles
    FROM dbo.Publicaciones AS p
    INNER JOIN dbo.Usuarios AS u
        ON u.IdUsuario = p.IdVendedor
    INNER JOIN dbo.Vehiculos AS v
        ON v.IdVehiculo = p.IdVehiculo
    WHERE p.Disponible = 1
      AND (@Marca IS NULL OR v.Marca = @Marca)
      AND (@TipoVehiculo IS NULL OR v.TipoVehiculo = @TipoVehiculo)
      AND (@Anio IS NULL OR v.Anio = @Anio)
      AND (@PrecioMaximo IS NULL OR v.PrecioVenta <= @PrecioMaximo)
    ORDER BY p.FechaPublicacion DESC;
END;
GO

/* =========================================================
   9. PROCEDIMIENTO PARA COMPLETAR UNA COMPRA
   Registra la compra y oculta el vehiculo del apartado de compras.
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.sp_RegistrarCompra
    @IdPublicacion INT,
    @IdComprador INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdVendedor INT;
    DECLARE @PrecioVenta DECIMAL(12,2);

    SELECT
        @IdVendedor = p.IdVendedor,
        @PrecioVenta = v.PrecioVenta
    FROM dbo.Publicaciones AS p
    INNER JOIN dbo.Vehiculos AS v
        ON v.IdVehiculo = p.IdVehiculo
    WHERE p.IdPublicacion = @IdPublicacion
      AND p.Disponible = 1;

    IF @PrecioVenta IS NULL
        THROW 50003, 'La publicacion no existe o el vehiculo ya no esta disponible.', 1;

    IF @IdVendedor = @IdComprador
        THROW 50004, 'El vendedor no puede comprar su propio vehiculo.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.Usuarios
        WHERE IdUsuario = @IdComprador AND Activo = 1
    )
        THROW 50005, 'El usuario comprador no existe o esta inactivo.', 1;

    BEGIN TRANSACTION;

    UPDATE dbo.Publicaciones
    SET Disponible = 0
    WHERE IdPublicacion = @IdPublicacion
      AND Disponible = 1;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50006, 'El vehiculo fue comprado por otro usuario.', 1;
    END;

    INSERT INTO dbo.Compras
        (IdPublicacion, IdComprador, PrecioPagado)
    VALUES
        (@IdPublicacion, @IdComprador, @PrecioVenta);

    COMMIT TRANSACTION;
END;
GO

/* =========================================================
   10. PRUEBAS OPCIONALES

   Ejecuta estas instrucciones una vez, despues de crear la base.
   Puedes seleccionarlas individualmente en SSMS.
   ========================================================= */

-- Registrar un vendedor:
-- EXEC dbo.sp_RegistrarUsuario
--     @Nombre = N'Vendedor de prueba',
--     @Edad = 25,
--     @Email = N'vendedor@carmarket.com',
--     @Telefono = N'6000-0001',
--     @Contrasena = N'Clave123';

-- Registrar un comprador:
-- EXEC dbo.sp_RegistrarUsuario
--     @Nombre = N'Comprador de prueba',
--     @Edad = 22,
--     @Email = N'comprador@carmarket.com',
--     @Telefono = N'6000-0002',
--     @Contrasena = N'Clave456';

-- Probar el inicio de sesion:
-- EXEC dbo.sp_IniciarSesion
--     @Email = N'vendedor@carmarket.com',
--     @Contrasena = N'Clave123';

-- Publicar un vehiculo usando el IdUsuario devuelto al registrar:
-- EXEC dbo.sp_PublicarVehiculo
--     @IdVendedor = 1,
--     @Marca = N'Toyota',
--     @Modelo = N'Corolla',
--     @Anio = 2020,
--     @TipoVehiculo = N'Sedan',
--     @PrecioOriginal = 23000.00,
--     @PrecioVenta = 14500.00,
--     @Kilometraje = 75000.00,
--     @Color = N'Blanco',
--     @Detalles = N'Buen estado y mantenimiento al dia.';

-- Mostrar todos los vehiculos disponibles:
-- EXEC dbo.sp_ListarPublicaciones;

-- Revisar directamente el contenido de las tablas:
-- SELECT * FROM dbo.Usuarios;
-- SELECT * FROM dbo.Vehiculos;
-- SELECT * FROM dbo.Publicaciones;
-- SELECT * FROM dbo.Compras;
GO
