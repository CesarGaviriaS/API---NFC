-- =====================================================
-- Script de Optimización de Índices
-- Para mejorar el rendimiento del sistema NFC SENA
-- =====================================================

USE [NFCSENA]
GO

-- ✅ ÍNDICE 1: Optimización para GetPendientes
-- Mejora las consultas que buscan el registro más reciente por dispositivo
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ElementoProceso_Elemento_QuedoEnSena_Proceso')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ElementoProceso_Elemento_QuedoEnSena_Proceso]
    ON [dbo].[ElementoProceso] ([IdElemento], [QuedoEnSena], [IdProceso] DESC)
    INCLUDE ([Validado])
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, 
          SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, 
          ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
    
    PRINT '✅ Índice IX_ElementoProceso_Elemento_QuedoEnSena_Proceso creado exitosamente'
END
ELSE
BEGIN
    PRINT '⚠️  Índice IX_ElementoProceso_Elemento_QuedoEnSena_Proceso ya existe'
END
GO

-- =====================================================
-- Verificación de Índices Existentes
-- =====================================================

PRINT ''
PRINT '📊 Índices existentes en ElementoProceso:'
PRINT '================================================'

SELECT 
    i.name AS NombreIndice,
    i.type_desc AS TipoIndice,
    STUFF((
        SELECT ', ' + c.name
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
        ORDER BY ic.key_ordinal
        FOR XML PATH('')
    ), 1, 2, '') AS ColumnasLlave,
    STUFF((
        SELECT ', ' + c.name
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
        ORDER BY ic.index_column_id
        FOR XML PATH('')
    ), 1, 2, '') AS ColumnasIncluidas
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('dbo.ElementoProceso')
  AND i.type_desc <> 'HEAP'
ORDER BY i.name

PRINT ''
PRINT '✅ Optimización completada'
PRINT '================================================'
