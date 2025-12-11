-- ========================================
-- CONSULTAS PARA VERIFICAR DATOS EN BD
-- ========================================
-- Estas consultas muestran TODOS los datos que se guardan en SQL Server
-- cuando se hacen ingresos y salidas

USE NFCSENA;
GO

-- ========================================
-- 1. VER TODOS LOS PROCESOS (Ingreso/Salida)
-- ========================================
SELECT 
    p.IdProceso,
    p.EstadoProceso,
    tp.Tipo AS TipoProceso,
    p.TipoPersona,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.Nombre + ' ' + a.Apellido
        WHEN p.TipoPersona = 'Usuario' THEN u.Nombre + ' ' + u.Apellido
    END AS NombreCompleto,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.NumeroDocumento
        WHEN p.TipoPersona = 'Usuario' THEN u.NumeroDocumento
    END AS Documento,
    p.TimeStampEntradaSalida AS Fecha,
    p.Observaciones
FROM Proceso p
LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
LEFT JOIN Aprendiz a ON p.IdAprendiz = a.IdAprendiz
LEFT JOIN Usuario u ON p.IdUsuario = u.IdUsuario
ORDER BY p.TimeStampEntradaSalida DESC;

-- ========================================
-- 2. VER DISPOSITIVOS POR PROCESO
-- ========================================
SELECT 
    ep.IdElementoProceso,
    ep.IdProceso,
    p.EstadoProceso,
    tp.Tipo AS TipoProceso,
    e.Serial,
    te.Tipo AS TipoElemento,
    e.Marca,
    e.Modelo,
    ep.Validado,
    ep.QuedoEnSena,
    e.EstaDentro
FROM ElementoProceso ep
INNER JOIN Proceso p ON ep.IdProceso = p.IdProceso
INNER JOIN Elemento e ON ep.IdElemento = e.IdElemento
INNER JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento
LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
ORDER BY ep.IdProceso DESC, ep.IdElementoProceso DESC;

-- ========================================
-- 3. VER REGISTROS NFC (Eventos de Ingreso/Salida)
-- ========================================
SELECT 
    r.IdRegistro,
    r.TipoRegistro,
    r.FechaRegistro,
    r.Estado,
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN 'Aprendiz'
        WHEN r.IdUsuario IS NOT NULL THEN 'Usuario'
    END AS TipoPersona,
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN a.Nombre + ' ' + a.Apellido
        WHEN r.IdUsuario IS NOT NULL THEN u.Nombre + ' ' + u.Apellido
    END AS NombreCompleto,
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN a.NumeroDocumento
        WHEN r.IdUsuario IS NOT NULL THEN u.NumeroDocumento
    END AS Documento
FROM RegistroNFC r
LEFT JOIN Aprendiz a ON r.IdAprendiz = a.IdAprendiz
LEFT JOIN Usuario u ON r.IdUsuario = u.IdUsuario
ORDER BY r.FechaRegistro DESC;

-- ========================================
-- 4. FLUJO COMPLETO - Unir Procesos + Dispositivos + Registros
-- ========================================
SELECT 
    p.IdProceso,
    p.EstadoProceso,
    tp.Tipo AS TipoProceso,
    p.TimeStampEntradaSalida,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.Nombre + ' ' + a.Apellido
        WHEN p.TipoPersona = 'Usuario' THEN u.Nombre + ' ' + u.Apellido
    END AS Persona,
    -- Contar dispositivos del proceso
    (SELECT COUNT(*) FROM ElementoProceso WHERE IdProceso = p.IdProceso) AS TotalDispositivos,
    (SELECT COUNT(*) FROM ElementoProceso WHERE IdProceso = p.IdProceso AND QuedoEnSena = 1) AS DispositivosQuedaron,
    (SELECT COUNT(*) FROM ElementoProceso WHERE IdProceso = p.IdProceso AND QuedoEnSena = 0) AS DispositivosSalieron,
    -- Ver si hay registro NFC asociado
    r.IdRegistro,
    r.TipoRegistro AS RegistroTipo,
    r.FechaRegistro
FROM Proceso p
LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
LEFT JOIN Aprendiz a ON p.IdAprendiz = a.IdAprendiz AND p.TipoPersona = 'Aprendiz'
LEFT JOIN Usuario u ON p.IdUsuario = u.IdUsuario AND p.TipoPersona = 'Usuario'
LEFT JOIN RegistroNFC r ON (r.IdAprendiz = p.IdAprendiz OR r.IdUsuario = p.IdUsuario)
    AND CAST(r.FechaRegistro AS DATE) = CAST(p.TimeStampEntradaSalida AS DATE)
WHERE p.TimeStampEntradaSalida >= DATEADD(day, -7, GETDATE()) -- Últimos 7 días
ORDER BY p.TimeStampEntradaSalida DESC;

-- ========================================
-- 5. DISPOSITIVOS QUE ESTÁN ACTUALMENTE DENTRO
-- ========================================
SELECT 
    e.IdElemento,
    e.Serial,
    te.Tipo AS TipoElemento,
    e.Marca,
    e.Modelo,
    e.TipoPropietario,
    CASE 
        WHEN e.TipoPropietario = 'Aprendiz' THEN a.Nombre + ' ' + a.Apellido
        WHEN e.TipoPropietario = 'Usuario' THEN u.Nombre + ' ' + u.Apellido
    END AS Propietario,
    e.EstaDentro,
    -- Ver último proceso
    (SELECT TOP 1 IdProceso FROM ElementoProceso 
     WHERE IdElemento = e.IdElemento 
     ORDER BY IdElementoProceso DESC) AS UltimoProceso
FROM Elemento e
INNER JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento
LEFT JOIN Aprendiz a ON e.IdPropietario = a.IdAprendiz AND e.TipoPropietario = 'Aprendiz'
LEFT JOIN Usuario u ON e.IdPropietario = u.IdUsuario AND e.TipoPropietario = 'Usuario'
WHERE e.EstaDentro = 1 AND e.Estado = 1
ORDER BY e.IdElemento DESC;

-- ========================================
-- 6. VER DETALLE DE UN PROCESO ESPECÍFICO
-- ========================================
-- Cambia el número 1 por el ID del proceso que quieres ver
DECLARE @IdProcesoConsulta INT = 1;

SELECT 
    'PROCESO' AS Tipo,
    p.IdProceso,
    tp.Tipo AS TipoProceso,
    p.EstadoProceso,
    p.TipoPersona,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.Nombre + ' ' + a.Apellido
        ELSE u.Nombre + ' ' + u.Apellido
    END AS Persona,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.NumeroDocumento
        ELSE u.NumeroDocumento
    END AS Documento,
    p.TimeStampEntradaSalida,
    p.Observaciones
FROM Proceso p
LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
LEFT JOIN Aprendiz a ON p.IdAprendiz = a.IdAprendiz
LEFT JOIN Usuario u ON p.IdUsuario = u.IdUsuario
WHERE p.IdProceso = @IdProcesoConsulta

UNION ALL

SELECT 
    'DISPOSITIVO' AS Tipo,
    ep.IdElementoProceso,
    e.Serial,
    CASE WHEN ep.QuedoEnSena = 1 THEN 'Quedó en SENA' ELSE 'Salió' END,
    te.Tipo,
    e.Marca,
    e.Modelo,
    NULL,
    CASE WHEN ep.Validado = 1 THEN 'Validado' ELSE 'No Validado' END
FROM ElementoProceso ep
INNER JOIN Elemento e ON ep.IdElemento = e.IdElemento
INNER JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento
WHERE ep.IdProceso = @IdProcesoConsulta;

-- ========================================
-- 7. VERIFICAR SI EXISTE DetalleRegistroNFC
-- ========================================
IF OBJECT_ID('dbo.DetalleRegistroNFC', 'U') IS NOT NULL
    SELECT 'La tabla DetalleRegistroNFC EXISTE en la base de datos' AS Mensaje
ELSE
    SELECT '⚠️ La tabla DetalleRegistroNFC NO EXISTE en la base de datos' AS Mensaje;

-- Si existe, mostrar sus datos
IF OBJECT_ID('dbo.DetalleRegistroNFC', 'U') IS NOT NULL
BEGIN
    SELECT * FROM DetalleRegistroNFC ORDER BY FechaHora DESC;
END

-- ========================================
-- 8. RESUMEN DE DATOS HOY
-- ========================================
SELECT 
    'Procesos Hoy' AS Concepto,
    COUNT(*) AS Cantidad
FROM Proceso
WHERE CAST(TimeStampEntradaSalida AS DATE) = CAST(GETDATE() AS DATE)

UNION ALL

SELECT 
    'Registros NFC Hoy',
    COUNT(*)
FROM RegistroNFC
WHERE CAST(FechaRegistro AS DATE) = CAST(GETDATE() AS DATE)

UNION ALL

SELECT 
    'Dispositivos Dentro',
    COUNT(*)
FROM Elemento
WHERE EstaDentro = 1

UNION ALL

SELECT 
    'Total Dispositivos Activos',
    COUNT(*)
FROM Elemento
WHERE Estado = 1;
