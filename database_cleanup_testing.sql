-- =====================================================
-- Script de Limpieza de Base de Datos para Pruebas
-- PRESERVA: TipoElemento, Ficha, Programa, TipoProceso
-- PRESERVA: Usuario "wilmar" (super admin)
-- ELIMINA: Todos los procesos, elementos, registros de prueba
-- =====================================================

USE [NFCSENA]
GO

SET QUOTED_IDENTIFIER ON
GO

PRINT '🧹 Iniciando limpieza de base de datos...'
PRINT '================================================'

-- =====================================================
-- PASO 1: Eliminar datos transaccionales
-- (en orden inverso de dependencias)
-- =====================================================

-- 1.1 Eliminar ElementoProceso (depende de Proceso y Elemento)
PRINT ''
PRINT '📦 Eliminando ElementoProceso...'
DELETE FROM [dbo].[ElementoProceso]
PRINT '   ✅ ElementoProceso eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- 1.2 Eliminar RegistroNFC
PRINT ''
PRINT '📝 Eliminando RegistroNFC...'
DELETE FROM [dbo].[RegistroNFC]
PRINT '   ✅ RegistroNFC eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- 1.3 Eliminar Proceso
PRINT ''
PRINT '🔄 Eliminando Proceso...'
DELETE FROM [dbo].[Proceso]
PRINT '   ✅ Proceso eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- 1.4 Eliminar TagAsignado
PRINT ''
PRINT '🏷️  Eliminando TagAsignado...'
DELETE FROM [dbo].[TagAsignado]
PRINT '   ✅ TagAsignado eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- 1.5 Eliminar Elemento
PRINT ''
PRINT '💾 Eliminando Elemento...'
DELETE FROM [dbo].[Elemento]
PRINT '   ✅ Elemento eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- =====================================================
-- PASO 2: Eliminar Aprendices
-- =====================================================
PRINT ''
PRINT '🎓 Eliminando Aprendiz...'
DELETE FROM [dbo].[Aprendiz]
PRINT '   ✅ Aprendiz eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- =====================================================
-- PASO 3: Eliminar Usuarios (EXCEPTO wilmar)
-- =====================================================
PRINT ''
PRINT '👤 Eliminando Usuario (excepto wilmar)...'
DELETE FROM [dbo].[Usuario]
WHERE Nombre != 'wilmar'  -- Preservar super admin
PRINT '   ✅ Usuarios eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- =====================================================
-- PASO 4: Verificar datos preservados
-- =====================================================
PRINT ''
PRINT '================================================'
PRINT '📊 VERIFICACIÓN DE DATOS PRESERVADOS'
PRINT '================================================'

DECLARE @TipoElementoCount INT
DECLARE @TipoProcesoCount INT
DECLARE @ProgramaCount INT
DECLARE @FichaCount INT
DECLARE @UsuarioCount INT

SELECT @TipoElementoCount = COUNT(*) FROM [dbo].[TipoElemento]
SELECT @TipoProcesoCount = COUNT(*) FROM [dbo].[TipoProceso]
SELECT @ProgramaCount = COUNT(*) FROM [dbo].[Programa]
SELECT @FichaCount = COUNT(*) FROM [dbo].[Ficha]
SELECT @UsuarioCount = COUNT(*) FROM [dbo].[Usuario]

PRINT ''
PRINT '✅ Datos de configuración preservados:'
PRINT '   • TipoElemento: ' + CAST(@TipoElementoCount AS VARCHAR) + ' registros'
PRINT '   • TipoProceso: ' + CAST(@TipoProcesoCount AS VARCHAR) + ' registros'
PRINT '   • Programa: ' + CAST(@ProgramaCount AS VARCHAR) + ' registros'
PRINT '   • Ficha: ' + CAST(@FichaCount AS VARCHAR) + ' registros'
PRINT '   • Usuario (admin): ' + CAST(@UsuarioCount AS VARCHAR) + ' registro(s)'

-- Mostrar el usuario preservado
PRINT ''
PRINT '👤 Usuarios preservados:'
SELECT Nombre, Apellido, Correo, Rol 
FROM [dbo].[Usuario]

-- =====================================================
-- PASO 5: Resetear IDs de identidad (OPCIONAL)
-- =====================================================
PRINT ''
PRINT '================================================'
PRINT '🔄 RESETEO DE IDs (OPCIONAL - descomentado por seguridad)'
PRINT '================================================'
PRINT 'Si deseas resetear los IDs, descomenta las líneas DBCC en el script'

-- DBCC CHECKIDENT ('[dbo].[Proceso]', RESEED, 0)
-- DBCC CHECKIDENT ('[dbo].[ElementoProceso]', RESEED, 0)
-- DBCC CHECKIDENT ('[dbo].[Elemento]', RESEED, 0)
-- DBCC CHECKIDENT ('[dbo].[RegistroNFC]', RESEED, 0)
-- DBCC CHECKIDENT ('[dbo].[Aprendiz]', RESEED, 0)
-- DBCC CHECKIDENT ('[dbo].[TagAsignado]', RESEED, 0)

PRINT ''
PRINT '================================================'
PRINT '✅ LIMPIEZA COMPLETADA EXITOSAMENTE'
PRINT '================================================'
PRINT 'La base de datos está lista para pruebas'
PRINT ''
