-- =============================================
-- Script de Migración: Hacer CodigoBarras Nullable en Aprendiz
-- Fecha: 2025-12-08
-- Descripción: Modifica la columna CodigoBarras en la tabla Aprendiz
--              para permitir valores NULL, alineando la BD con el modelo C#
-- =============================================

USE [NFCSENA]
GO

-- Verificar si hay datos antes de la modificación
PRINT '=== Estado actual de la tabla Aprendiz ==='
SELECT COUNT(*) AS TotalAprendices, 
       COUNT(CodigoBarras) AS ConCodigoBarras,
       COUNT(*) - COUNT(CodigoBarras) AS SinCodigoBarras
FROM dbo.Aprendiz;
GO

-- Modificar la columna CodigoBarras para permitir NULL
PRINT '=== Modificando columna CodigoBarras ==='
ALTER TABLE dbo.Aprendiz
ALTER COLUMN CodigoBarras VARCHAR(100) NULL;
GO

-- Verificar el cambio
PRINT '=== Verificando cambio ==='
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Aprendiz' 
  AND COLUMN_NAME = 'CodigoBarras';
GO

PRINT '=== Migración completada exitosamente ==='
GO
