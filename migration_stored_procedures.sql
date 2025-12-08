-- ==================================================================
-- STORED PROCEDURES Y TRIGGERS - Sistema NFC
-- Fecha: 2025-12-08
-- Descripción: Procedures para gestionar DetalleRegistroNFC
-- ==================================================================

USE [NFCSENA]
GO

PRINT '=========================================='
PRINT 'CREANDO STORED PROCEDURES Y TRIGGERS'
PRINT '=========================================='
PRINT ''

-- ==================================================================
-- STORED PROCEDURE: Registrar Detalle de Ingreso
-- ==================================================================

PRINT '📋 Creando SP: usp_RegistrarDetalleIngreso...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RegistrarDetalleIngreso]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_RegistrarDetalleIngreso]
GO

CREATE PROCEDURE [dbo].[usp_RegistrarDetalleIngreso]
    @IdRegistroNFC INT,
    @IdProceso INT,
    @IdElemento INT,
    @Validado BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verificar que no exista ya un detalle para este registro+elemento
        IF EXISTS (
            SELECT 1 FROM DetalleRegistroNFC 
            WHERE IdRegistroNFC = @IdRegistroNFC 
              AND IdElemento = @IdElemento
        )
        BEGIN
            RAISERROR('Ya existe un detalle para este registro y elemento', 16, 1)
            RETURN
        END

        -- Insertar el detalle
        INSERT INTO [dbo].[DetalleRegistroNFC] (
            IdRegistroNFC,
            IdElemento,
            IdProceso,
            Accion,
            FechaHora,
            Validado
        )
        VALUES (
            @IdRegistroNFC,
            @IdElemento,
            @IdProceso,
            'Ingresó',
            GETDATE(),
            @Validado
        )

        PRINT '   ✅ Detalle de ingreso registrado - Elemento: ' + CAST(@IdElemento AS VARCHAR(10))
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO

PRINT '   ✅ usp_RegistrarDetalleIngreso creado'
GO

-- ==================================================================
-- STORED PROCEDURE: Registrar Detalle de Salida
-- ==================================================================

PRINT ''
PRINT '📋 Creando SP: usp_RegistrarDetalleSalida...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RegistrarDetalleSalida]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_RegistrarDetalleSalida]
GO

CREATE PROCEDURE [dbo].[usp_RegistrarDetalleSalida]
    @IdRegistroNFC INT,
    @IdProceso INT,
    @IdElemento INT,
    @QuedoEnSena BIT = 0,
    @Validado BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @Accion VARCHAR(20)
        
        -- Determinar la acción basándose en si quedó en SENA
        IF @QuedoEnSena = 1
            SET @Accion = 'Quedó'
        ELSE
            SET @Accion = 'Salió'

        -- Insertar el detalle
        INSERT INTO [dbo].[DetalleRegistroNFC] (
            IdRegistroNFC,
            IdElemento,
            IdProceso,
            Accion,
            FechaHora,
            Validado
        )
        VALUES (
            @IdRegistroNFC,
            @IdElemento,
            @IdProceso,
            @Accion,
            GETDATE(),
            @Validado
        )

        PRINT '   ✅ Detalle de salida registrado - Elemento: ' + CAST(@IdElemento AS VARCHAR(10)) + ' - Acción: ' + @Accion
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO

PRINT '   ✅ usp_RegistrarDetalleSalida creado'
GO

-- ==================================================================
-- STORED PROCEDURE: Obtener Historial de Elemento
-- ==================================================================

PRINT ''
PRINT '📋 Creando SP: usp_GetHistorialElemento...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetHistorialElemento]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_GetHistorialElemento]
GO

CREATE PROCEDURE [dbo].[usp_GetHistorialElemento]
    @IdElemento INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        d.IdDetalleRegistro,
        d.Accion,
        d.FechaHora,
        d.Validado,
        r.TipoRegistro,
        r.FechaRegistro,
        p.IdProceso,
        p.EstadoProceso,
        e.Serial,
        e.Marca,
        e.Modelo,
        te.Tipo AS TipoElemento
    FROM DetalleRegistroNFC d
    INNER JOIN RegistroNFC r ON d.IdRegistroNFC = r.IdRegistro
    INNER JOIN Proceso p ON d.IdProceso = p.IdProceso
    INNER JOIN Elemento e ON d.IdElemento = e.IdElemento
    LEFT JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento
    WHERE d.IdElemento = @IdElemento
    ORDER BY d.FechaHora DESC
END
GO

PRINT '   ✅ usp_GetHistorialElemento creado'
GO

-- ==================================================================
-- STORED PROCEDURE: Obtener Detalles por Registro
-- ==================================================================

PRINT ''
PRINT '📋 Creando SP: usp_GetDetallesPorRegistro...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetDetallesPorRegistro]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_GetDetallesPorRegistro]
GO

CREATE PROCEDURE [dbo].[usp_GetDetallesPorRegistro]
    @IdRegistroNFC INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        d.IdDetalleRegistro,
        d.IdElemento,
        d.Accion,
        d.FechaHora,
        d.Validado,
        e.Serial,
        e.Marca,
        e.Modelo,
        te.Tipo AS TipoElemento
    FROM DetalleRegistroNFC d
    INNER JOIN Elemento e ON d.IdElemento = e.IdElemento
    LEFT JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento
    WHERE d.IdRegistroNFC = @IdRegistroNFC
    ORDER BY d.FechaHora ASC
END
GO

PRINT '   ✅ usp_GetDetallesPorRegistro creado'
GO

-- ==================================================================
-- STORED PROCEDURE: Registrar Ingreso Completo
-- Registra el RegistroNFC y todos los detalles en una transacción
-- ==================================================================

PRINT ''
PRINT '📋 Creando SP: usp_RegistrarIngresoCompleto...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RegistrarIngresoCompleto]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_RegistrarIngresoCompleto]
GO

CREATE PROCEDURE [dbo].[usp_RegistrarIngresoCompleto]
    @IdProceso INT,
    @IdAprendiz INT = NULL,
    @IdUsuario INT = NULL,
    @ElementosXml XML  -- XML con lista de elementos: <Elementos><Elemento IdElemento="1" Validado="1"/></Elementos>
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION
    BEGIN TRY
        DECLARE @IdRegistroNFC INT

        -- 1. Crear el RegistroNFC
        INSERT INTO RegistroNFC (IdAprendiz, IdUsuario, TipoRegistro, FechaRegistro, Estado, IdProceso)
        VALUES (
            @IdAprendiz,
            @IdUsuario,
            'Ingreso',
            GETDATE(),
            'Activo',
            @IdProceso
        )

        SET @IdRegistroNFC = SCOPE_IDENTITY()
        PRINT '   ✅ RegistroNFC creado: ' + CAST(@IdRegistroNFC AS VARCHAR(10))

        -- 2. Insertar detalles desde XML
        INSERT INTO DetalleRegistroNFC (IdRegistroNFC, IdElemento, IdProceso, Accion, FechaHora, Validado)
        SELECT 
            @IdRegistroNFC,
            T.c.value('@IdElemento', 'INT'),
            @IdProceso,
            'Ingresó',
            GETDATE(),
            T.c.value('@Validado', 'BIT')
        FROM @ElementosXml.nodes('/Elementos/Elemento') AS T(c)

        DECLARE @CantidadDetalles INT = @@ROWCOUNT
        PRINT '   ✅ ' + CAST(@CantidadDetalles AS VARCHAR(10)) + ' detalles registrados'

        COMMIT TRANSACTION
        
        -- Retornar el ID del registro creado
        SELECT @IdRegistroNFC AS IdRegistroNFC, @CantidadDetalles AS CantidadDetalles
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO

PRINT '   ✅ usp_RegistrarIngresoCompleto creado'
GO

-- ==================================================================
-- STORED PROCEDURE: Registrar Salida Completa
-- ==================================================================

PRINT ''
PRINT '📋 Creando SP: usp_RegistrarSalidaCompleta...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_RegistrarSalidaCompleta]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_RegistrarSalidaCompleta]
GO

CREATE PROCEDURE [dbo].[usp_RegistrarSalidaCompleta]
    @IdProceso INT,
    @IdAprendiz INT = NULL,
    @IdUsuario INT = NULL,
    @ElementosXml XML  -- XML: <Elementos><Elemento IdElemento="1" QuedoEnSena="0" Validado="1"/></Elementos>
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION
    BEGIN TRY
        DECLARE @IdRegistroNFC INT

        -- 1. Crear el RegistroNFC
        INSERT INTO RegistroNFC (IdAprendiz, IdUsuario, TipoRegistro, FechaRegistro, Estado, IdProceso)
        VALUES (
            @IdAprendiz,
            @IdUsuario,
            'Salida',
            GETDATE(),
            'Activo',
            @IdProceso
        )

        SET @IdRegistroNFC = SCOPE_IDENTITY()
        PRINT '   ✅ RegistroNFC creado: ' + CAST(@IdRegistroNFC AS VARCHAR(10))

        -- 2. Insertar detalles desde XML
        INSERT INTO DetalleRegistroNFC (IdRegistroNFC, IdElemento, IdProceso, Accion, FechaHora, Validado)
        SELECT 
            @IdRegistroNFC,
            T.c.value('@IdElemento', 'INT'),
            @IdProceso,
            CASE WHEN T.c.value('@QuedoEnSena', 'BIT') = 1 THEN 'Quedó' ELSE 'Salió' END,
            GETDATE(),
            T.c.value('@Validado', 'BIT')
        FROM @ElementosXml.nodes('/Elementos/Elemento') AS T(c)

        DECLARE @CantidadDetalles INT = @@ROWCOUNT
        PRINT '   ✅ ' + CAST(@CantidadDetalles AS VARCHAR(10)) + ' detalles registrados'

        COMMIT TRANSACTION
        
        -- Retornar el ID del registro creado
        SELECT @IdRegistroNFC AS IdRegistroNFC, @CantidadDetalles AS CantidadDetalles
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO

PRINT '   ✅ usp_RegistrarSalidaCompleta creado'
GO

PRINT ''
PRINT '=========================================='
PRINT '✅ STORED PROCEDURES CREADOS'
PRINT '=========================================='
PRINT ''
PRINT 'Stored Procedures creados:'
PRINT '  • usp_RegistrarDetalleIngreso'
PRINT '  • usp_RegistrarDetalleSalida'
PRINT '  • usp_GetHistorialElemento'
PRINT '  • usp_GetDetallesPorRegistro'
PRINT '  • usp_RegistrarIngresoCompleto'
PRINT '  • usp_RegistrarSalidaCompleta'
PRINT ''
PRINT 'Siguiente paso: Actualizar modelos C# y controladores'
PRINT ''
GO
