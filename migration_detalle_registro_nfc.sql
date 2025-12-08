-- ==================================================================
-- MIGRACIÓN: Sistema NFC - Nueva Tabla DetalleRegistroNFC
-- Fecha: 2025-12-08
-- Descripción: Agrega trazabilidad completa de dispositivos
-- ==================================================================

USE [NFCSENA]
GO

PRINT '=========================================='
PRINT 'INICIANDO MIGRACIÓN - DetalleRegistroNFC'
PRINT '=========================================='
PRINT ''

-- ==================================================================
-- PASO 1: Crear nueva tabla DetalleRegistroNFC
-- ==================================================================

PRINT '📋 PASO 1: Creando tabla DetalleRegistroNFC...'
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DetalleRegistroNFC]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DetalleRegistroNFC](
        [IdDetalleRegistro] [int] IDENTITY(1,1) NOT NULL,
        [IdRegistroNFC] [int] NOT NULL,
        [IdElemento] [int] NOT NULL,
        [IdProceso] [int] NOT NULL,
        [Accion] [varchar](20) NOT NULL,
        [FechaHora] [datetime] NOT NULL,
        [Validado] [bit] NULL,
        
        CONSTRAINT [PK_DetalleRegistroNFC] PRIMARY KEY CLUSTERED 
        (
            [IdDetalleRegistro] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    PRINT '   ✅ Tabla DetalleRegistroNFC creada'
END
ELSE
BEGIN
    PRINT '   ⚠️ Tabla DetalleRegistroNFC ya existe'
END
GO

-- ==================================================================
-- PASO 2: Agregar columna IdProceso a RegistroNFC
-- ==================================================================

PRINT ''
PRINT '📋 PASO 2: Agregando columna IdProceso a RegistroNFC...'
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RegistroNFC]') AND name = 'IdProceso')
BEGIN
    ALTER TABLE [dbo].[RegistroNFC]
    ADD [IdProceso] [int] NULL

    PRINT '   ✅ Columna IdProceso agregada'
END
ELSE
BEGIN
    PRINT '   ⚠️ Columna IdProceso ya existe'
END
GO

-- ==================================================================
-- PASO 3: Crear Foreign Keys
-- ==================================================================

PRINT ''
PRINT '📋 PASO 3: Creando Foreign Keys...'
GO

-- FK: RegistroNFC -> Proceso
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RegistroNFC_Proceso]'))
BEGIN
    ALTER TABLE [dbo].[RegistroNFC]
    ADD CONSTRAINT [FK_RegistroNFC_Proceso] 
        FOREIGN KEY([IdProceso]) REFERENCES [dbo].[Proceso]([IdProceso])
    
    PRINT '   ✅ FK_RegistroNFC_Proceso creada'
END
ELSE
BEGIN
    PRINT '   ⚠️ FK_RegistroNFC_Proceso ya existe'
END
GO

-- FK: DetalleRegistroNFC -> RegistroNFC
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DetalleRegistroNFC_RegistroNFC]'))
BEGIN
    ALTER TABLE [dbo].[DetalleRegistroNFC]
    ADD CONSTRAINT [FK_DetalleRegistroNFC_RegistroNFC] 
        FOREIGN KEY([IdRegistroNFC]) REFERENCES [dbo].[RegistroNFC]([IdRegistro])
    
    PRINT '   ✅ FK_DetalleRegistroNFC_RegistroNFC creada'
END
ELSE
BEGIN
    PRINT '   ⚠️ FK_DetalleRegistroNFC_RegistroNFC ya existe'
END
GO

-- FK: DetalleRegistroNFC -> Elemento
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DetalleRegistroNFC_Elemento]'))
BEGIN
    ALTER TABLE [dbo].[DetalleRegistroNFC]
    ADD CONSTRAINT [FK_DetalleRegistroNFC_Elemento] 
        FOREIGN KEY([IdElemento]) REFERENCES [dbo].[Elemento]([IdElemento])
    
    PRINT '   ✅ FK_DetalleRegistroNFC_Elemento creada'
END
ELSE
BEGIN
    PRINT '   ⚠️ FK_DetalleRegistroNFC_Elemento ya existe'
END
GO

-- FK: DetalleRegistroNFC -> Proceso
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DetalleRegistroNFC_Proceso]'))
BEGIN
    ALTER TABLE [dbo].[DetalleRegistroNFC]
    ADD CONSTRAINT [FK_DetalleRegistroNFC_Proceso] 
        FOREIGN KEY([IdProceso]) REFERENCES [dbo].[Proceso]([IdProceso])
    
    PRINT '   ✅ FK_DetalleRegistroNFC_Proceso creada'
END
ELSE
BEGIN
    PRINT '   ⚠️ FK_DetalleRegistroNFC_Proceso ya existe'
END
GO

-- ==================================================================
-- PASO 4: Agregar Constraints
-- ==================================================================

PRINT ''
PRINT '📋 PASO 4: Agregando Constraints...'
GO

-- Check constraint para Accion
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_DetalleRegistroNFC_Accion]'))
BEGIN
    ALTER TABLE [dbo].[DetalleRegistroNFC]
    ADD CONSTRAINT [CK_DetalleRegistroNFC_Accion] 
        CHECK ([Accion] IN ('Ingresó', 'Salió', 'Quedó'))
    
    PRINT '   ✅ CK_DetalleRegistroNFC_Accion creada'
END
ELSE
BEGIN
    PRINT '   ⚠️ CK_DetalleRegistroNFC_Accion ya existe'
END
GO

-- Default para FechaHora
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_DetalleRegistroNFC_FechaHora]'))
BEGIN
    ALTER TABLE [dbo].[DetalleRegistroNFC]
    ADD CONSTRAINT [DF_DetalleRegistroNFC_FechaHora] DEFAULT (GETDATE()) FOR [FechaHora]
    
    PRINT '   ✅ DF_DetalleRegistroNFC_FechaHora creada'
END
ELSE
BEGIN
    PRINT '   ⚠️ DF_DetalleRegistroNFC_FechaHora ya existe'
END
GO

-- Default para Validado
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_DetalleRegistroNFC_Validado]'))
BEGIN
    ALTER TABLE [dbo].[DetalleRegistroNFC]
    ADD CONSTRAINT [DF_DetalleRegistroNFC_Validado] DEFAULT (0) FOR [Validado]
    
    PRINT '   ✅ DF_DetalleRegistroNFC_Validado creada'
END
ELSE
BEGIN
    PRINT '   ⚠️ DF_DetalleRegistroNFC_Validado ya existe'
END
GO

-- ==================================================================
-- PASO 5: Crear Índices para optimización
-- ==================================================================

PRINT ''
PRINT '📋 PASO 5: Creando índices...'
GO

-- Índice en IdRegistroNFC
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DetalleRegistroNFC]') AND name = N'IX_DetalleRegistroNFC_RegistroNFC')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DetalleRegistroNFC_RegistroNFC]
    ON [dbo].[DetalleRegistroNFC] ([IdRegistroNFC] ASC)
    
    PRINT '   ✅ IX_DetalleRegistroNFC_RegistroNFC creado'
END
ELSE
BEGIN
    PRINT '   ⚠️ IX_DetalleRegistroNFC_RegistroNFC ya existe'
END
GO

-- Índice en IdElemento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DetalleRegistroNFC]') AND name = N'IX_DetalleRegistroNFC_Elemento')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DetalleRegistroNFC_Elemento]
    ON [dbo].[DetalleRegistroNFC] ([IdElemento] ASC)
    
    PRINT '   ✅ IX_DetalleRegistroNFC_Elemento creado'
END
ELSE
BEGIN
    PRINT '   ⚠️ IX_DetalleRegistroNFC_Elemento ya existe'
END
GO

-- Índice en IdProceso
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DetalleRegistroNFC]') AND name = N'IX_DetalleRegistroNFC_Proceso')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DetalleRegistroNFC_Proceso]
    ON [dbo].[DetalleRegistroNFC] ([IdProceso] ASC)
    
    PRINT '   ✅ IX_DetalleRegistroNFC_Proceso creado'
END
ELSE
BEGIN
    PRINT '   ⚠️ IX_DetalleRegistroNFC_Proceso ya existe'
END
GO

-- Índice en IdProceso de RegistroNFC
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RegistroNFC]') AND name = N'IX_RegistroNFC_Proceso')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RegistroNFC_Proceso]
    ON [dbo].[RegistroNFC] ([IdProceso] ASC)
    
    PRINT '   ✅ IX_RegistroNFC_Proceso creado'
END
ELSE
BEGIN
    PRINT '   ⚠️ IX_RegistroNFC_Proceso ya existe'
END
GO

-- Índice compuesto para consultas comunes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DetalleRegistroNFC]') AND name = N'IX_DetalleRegistroNFC_Elemento_Fecha')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DetalleRegistroNFC_Elemento_Fecha]
    ON [dbo].[DetalleRegistroNFC] ([IdElemento] ASC, [FechaHora] DESC)
    
    PRINT '   ✅ IX_DetalleRegistroNFC_Elemento_Fecha creado'
END
ELSE
BEGIN
    PRINT '   ⚠️ IX_DetalleRegistroNFC_Elemento_Fecha ya existe'
END
GO

PRINT ''
PRINT '=========================================='
PRINT '✅ MIGRACIÓN COMPLETADA EXITOSAMENTE'
PRINT '=========================================='
PRINT ''
PRINT 'Resumen:'
PRINT '  • Tabla DetalleRegistroNFC creada'
PRINT '  • Columna IdProceso agregada a RegistroNFC'
PRINT '  • 4 Foreign Keys creadas'
PRINT '  • 3 Constraints creadas'
PRINT '  • 5 Índices creados'
PRINT ''
PRINT 'Siguiente paso: Ejecutar migration_stored_procedures.sql'
PRINT ''
GO
