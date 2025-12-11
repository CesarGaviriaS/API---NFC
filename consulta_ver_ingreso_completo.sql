-- ========================================
-- CONSULTA COMPLETA: Ver Ingreso/Salida con Dispositivos (como GestionNFC)
-- ========================================
USE NFCSENA;
GO

-- ========================================
-- EJEMPLO COMPLETO: Ver un proceso con todos sus dispositivos
-- ========================================

-- 📋 REPORTE DETALLADO DE PROCESO (Similar a GestionNFC)
SELECT 
    -- Información del Proceso
    p.IdProceso,
    tp.Tipo AS TipoProceso,
    p.EstadoProceso,
    p.TimeStampEntradaSalida AS FechaHora,
    
    -- Información de la Persona
    p.TipoPersona,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.Nombre + ' ' + a.Apellido
        WHEN p.TipoPersona = 'Usuario' THEN u.Nombre + ' ' + u.Apellido
    END AS NombreCompleto,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.NumeroDocumento
        WHEN p.TipoPersona = 'Usuario' THEN u.NumeroDocumento
    END AS Documento,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.Correo
        WHEN p.TipoPersona = 'Usuario' THEN u.Correo
    END AS Correo,
    
    -- Información del Dispositivo
    e.IdElemento,
    te.Tipo AS TipoDispositivo,
    e.Marca,
    e.Modelo,
    e.Serial,
    
    -- Estado del Dispositivo en el Proceso
    ep.Validado,
    ep.QuedoEnSena,
    e.EstaDentro,
    
    -- RegistroNFC asociado (si existe)
    r.IdRegistro,
    r.TipoRegistro AS TipoRegistroNFC,
    r.FechaRegistro

FROM Proceso p

-- Tipo de Proceso (Ingreso/Salida)
LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso

-- Datos de la Persona
LEFT JOIN Aprendiz a ON p.IdAprendiz = a.IdAprendiz AND p.TipoPersona = 'Aprendiz'
LEFT JOIN Usuario u ON p.IdUsuario = u.IdUsuario AND p.TipoPersona = 'Usuario'

-- Dispositivos del Proceso
LEFT JOIN ElementoProceso ep ON ep.IdProceso = p.IdProceso
LEFT JOIN Elemento e ON ep.IdElemento = e.IdElemento
LEFT JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento

-- Registro NFC (evento completado)
LEFT JOIN RegistroNFC r ON 
    (r.IdAprendiz = p.IdAprendiz OR r.IdUsuario = p.IdUsuario)
    AND CAST(r.FechaRegistro AS DATE) = CAST(p.TimeStampEntradaSalida AS DATE)
    AND r.TipoRegistro = tp.Tipo

-- Filtrar por fecha reciente
WHERE p.TimeStampEntradaSalida >= DATEADD(day, -7, GETDATE())

-- Ordenar
ORDER BY p.TimeStampEntradaSalida DESC, p.IdProceso DESC, e.Serial;

GO

-- ========================================
-- VERSIÓN RESUMIDA: Contar dispositivos por proceso
-- ========================================

SELECT 
    p.IdProceso,
    tp.Tipo AS TipoProceso,
    p.EstadoProceso,
    p.TimeStampEntradaSalida,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.Nombre + ' ' + a.Apellido
        ELSE u.Nombre + ' ' + u.Apellido
    END AS Persona,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.NumeroDocumento
        ELSE u.NumeroDocumento
    END AS Documento,
    
    -- Total de dispositivos
    COUNT(ep.IdElementoProceso) AS TotalDispositivos,
    
    -- Lista de dispositivos (concatenada)
    STRING_AGG(te.Tipo + ' ' + e.Serial, ', ') AS Dispositivos
    
FROM Proceso p
LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
LEFT JOIN Aprendiz a ON p.IdAprendiz = a.IdAprendiz
LEFT JOIN Usuario u ON p.IdUsuario = u.IdUsuario
LEFT JOIN ElementoProceso ep ON ep.IdProceso = p.IdProceso
LEFT JOIN Elemento e ON ep.IdElemento = e.IdElemento
LEFT JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento

WHERE p.TimeStampEntradaSalida >= DATEADD(day, -7, GETDATE())

GROUP BY 
    p.IdProceso,
    tp.Tipo,
    p.EstadoProceso,
    p.TimeStampEntradaSalida,
    p.TipoPersona,
    a.Nombre,
    a.Apellido,
    a.NumeroDocumento,
    u.Nombre,
    u.Apellido,
    u.NumeroDocumento

ORDER BY p.TimeStampEntradaSalida DESC;

GO

-- ========================================
-- VER UN PROCESO ESPECÍFICO (Cambiar el ID)
-- ========================================

DECLARE @IdProcesoEjemplo INT = (SELECT TOP 1 IdProceso FROM Proceso ORDER BY TimeStampEntradaSalida DESC);

PRINT '================================================';
PRINT 'PROCESO #' + CAST(@IdProcesoEjemplo AS VARCHAR(10));
PRINT '================================================';
PRINT '';

-- Información del Proceso
SELECT 
    '📋 INFORMACIÓN DEL PROCESO' AS Seccion,
    p.IdProceso,
    tp.Tipo AS TipoProceso,
    p.EstadoProceso,
    FORMAT(p.TimeStampEntradaSalida, 'dd/MM/yyyy HH:mm:ss') AS FechaHora,
    p.TipoPersona,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.Nombre + ' ' + a.Apellido
        ELSE u.Nombre + ' ' + u.Apellido
    END AS NombreCompleto,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.NumeroDocumento
        ELSE u.NumeroDocumento
    END AS Documento
FROM Proceso p
LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
LEFT JOIN Aprendiz a ON p.IdAprendiz = a.IdAprendiz
LEFT JOIN Usuario u ON p.IdUsuario = u.IdUsuario
WHERE p.IdProceso = @IdProcesoEjemplo;

PRINT '';
PRINT '📦 DISPOSITIVOS REGISTRADOS:';
PRINT '';

-- Dispositivos del Proceso
SELECT 
    ROW_NUMBER() OVER (ORDER BY e.Serial) AS '#',
    te.Tipo AS TipoDispositivo,
    e.Marca,
    e.Modelo,
    e.Serial,
    CASE WHEN ep.Validado = 1 THEN '✓ Validado' ELSE '✗ No Validado' END AS Estado,
    CASE WHEN ep.QuedoEnSena = 1 THEN '🏢 Quedó en SENA' ELSE '🏠 Salió' END AS Accion,
    CASE WHEN e.EstaDentro = 1 THEN '✓ Dentro' ELSE '✗ Fuera' END AS Ubicacion
FROM ElementoProceso ep
INNER JOIN Elemento e ON ep.IdElemento = e.IdElemento
INNER JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento
WHERE ep.IdProceso = @IdProcesoEjemplo
ORDER BY te.Tipo, e.Serial;

GO

-- ========================================
-- FLUJO COMPLETO COMO EN FlujoNFC.cshtml
-- ========================================

SELECT 
    FORMAT(p.TimeStampEntradaSalida, 'dd/MM/yyyy') AS Fecha,
    FORMAT(p.TimeStampEntradaSalida, 'HH:mm:ss') AS Hora,
    tp.Tipo AS TipoRegistro,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.Nombre + ' ' + a.Apellido
        ELSE u.Nombre + ' ' + u.Apellido
    END AS Persona,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN 'Aprendiz'
        ELSE 'Usuario'
    END AS Tipo,
    CASE 
        WHEN p.TipoPersona = 'Aprendiz' THEN a.NumeroDocumento
        ELSE u.NumeroDocumento
    END AS Documento,
    
    -- Lista de dispositivos con acción
    STRING_AGG(
        te.Tipo + ' ' + e.Serial + 
        CASE WHEN ep.QuedoEnSena = 1 THEN ' (Quedó)' ELSE ' (Salió)' END,
        ', '
    ) AS Dispositivos,
    
    p.EstadoProceso

FROM Proceso p
LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
LEFT JOIN Aprendiz a ON p.IdAprendiz = a.IdAprendiz
LEFT JOIN Usuario u ON p.IdUsuario = u.IdUsuario
LEFT JOIN ElementoProceso ep ON ep.IdProceso = p.IdProceso
LEFT JOIN Elemento e ON ep.IdElemento = e.IdElemento
LEFT JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento

WHERE 
    p.TimeStampEntradaSalida >= CAST(GETDATE() AS DATE)  -- Solo hoy
    AND p.EstadoProceso IN ('EnCurso', 'Cerrado')  -- Completados

GROUP BY 
    p.TimeStampEntradaSalida,
    tp.Tipo,
    p.TipoPersona,
    a.Nombre,
    a.Apellido,
    a.NumeroDocumento,
    u.Nombre,
    u.Apellido,
    u.NumeroDocumento,
    p.EstadoProceso

ORDER BY p.TimeStampEntradaSalida DESC;

GO

-- ========================================
-- EJEMPLO DE SALIDA ESPERADA
-- ========================================
/*
📋 INFORMACIÓN DEL PROCESO
IdProceso: 123
TipoProceso: Ingreso
EstadoProceso: EnCurso
FechaHora: 10/12/2025 14:30:00
TipoPersona: Aprendiz
NombreCompleto: Juan Pérez
Documento: 1010

📦 DISPOSITIVOS REGISTRADOS:
# | TipoDispositivo | Marca    | Modelo      | Serial      | Estado      | Accion          | Ubicacion
1 | Laptop          | Dell     | Inspiron    | SN12345     | ✓ Validado  | 🏠 Salió        | ✗ Fuera
2 | Mouse           | Logitech | M185        | SN67890     | ✓ Validado  | 🏢 Quedó SENA   | ✓ Dentro
3 | Audífonos       | Sony     | WH-1000XM4  | SN11111     | ✓ Validado  | 🏠 Salió        | ✗ Fuera
*/
