-- ========================================
-- CONSULTA EXACTA DE FlujoNFC.cshtml
-- Muestra todos los datos en UNA SOLA TABLA
-- ========================================
USE NFCSENA;
GO

-- Esta consulta replica exactamente lo que muestra el reporte FlujoNFC
-- con botones de exportar a PDF y Word

WITH ProcesoConDispositivos AS (
    -- Agrupar dispositivos por proceso
    SELECT 
        ep.IdProceso,
        STRING_AGG(
            CONCAT(
                COALESCE(te.Tipo, 'Dispositivo'),
                CASE 
                    WHEN e.Marca IS NOT NULL OR e.Modelo IS NOT NULL 
                    THEN ' ' + COALESCE(e.Marca + ' ', '') + COALESCE(e.Modelo, '')
                    ELSE ''
                END,
                ' (S/N ', COALESCE(e.Serial, 'N/A'), ')'
            ),
            ', '
        ) AS DispositivosTexto
    FROM ElementoProceso ep
    INNER JOIN Elemento e ON ep.IdElemento = e.IdElemento
    LEFT JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento
    WHERE ep.Validado = 1
    GROUP BY ep.IdProceso
)

SELECT 
    -- Fecha y Hora
    FORMAT(r.FechaRegistro, 'dd/MM/yyyy') AS Fecha,
    FORMAT(r.FechaRegistro, 'HH:mm:ss') AS Hora,
    
    -- Tipo de Registro
    r.TipoRegistro,
    
    -- Información de la Persona
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN a.Nombre + ' ' + a.Apellido
        WHEN r.IdUsuario IS NOT NULL THEN u.Nombre + ' ' + u.Apellido
        ELSE 'N/A'
    END AS NombreCompleto,
    
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN 'Aprendiz'
        WHEN r.IdUsuario IS NOT NULL THEN 'Usuario'
        ELSE 'N/A'
    END AS TipoPersona,
    
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN a.TipoDocumento + ' ' + a.NumeroDocumento
        WHEN r.IdUsuario IS NOT NULL THEN u.TipoDocumento + ' ' + u.NumeroDocumento
        ELSE 'N/A'
    END AS Documento,
    
    -- Dispositivos (lista concatenada)
    COALESCE(pcd.DispositivosTexto, 'Sin dispositivos') AS Dispositivos,
    
    -- Estado del Proceso
    p.EstadoProceso,
    
    -- IDs para referencia
    r.IdRegistro,
    p.IdProceso

FROM RegistroNFC r

-- Join con Aprendiz
LEFT JOIN Aprendiz a ON r.IdAprendiz = a.IdAprendiz

-- Join con Usuario
LEFT JOIN Usuario u ON r.IdUsuario = u.IdUsuario

-- Buscar el proceso asociado
LEFT JOIN Proceso p ON 
    (
        (r.IdAprendiz IS NOT NULL AND p.IdAprendiz = r.IdAprendiz AND p.TipoPersona = 'Aprendiz') OR
        (r.IdUsuario IS NOT NULL AND p.IdUsuario = r.IdUsuario AND p.TipoPersona = 'Usuario')
    )
    AND p.TimeStampEntradaSalida <= r.FechaRegistro
    AND p.IdProceso = (
        -- Obtener el proceso más reciente antes del registro
        SELECT TOP 1 p2.IdProceso
        FROM Proceso p2
        WHERE 
            (
                (r.IdAprendiz IS NOT NULL AND p2.IdAprendiz = r.IdAprendiz AND p2.TipoPersona = 'Aprendiz') OR
                (r.IdUsuario IS NOT NULL AND p2.IdUsuario = r.IdUsuario AND p2.TipoPersona = 'Usuario')
            )
            AND p2.TimeStampEntradaSalida <= r.FechaRegistro
        ORDER BY p2.TimeStampEntradaSalida DESC
    )

-- Join con dispositivos agrupados
LEFT JOIN ProcesoConDispositivos pcd ON pcd.IdProceso = p.IdProceso

-- Filtros (puedes cambiarlos según necesites)
WHERE 
    r.FechaRegistro >= CAST(GETDATE() AS DATE)  -- Solo hoy
    -- r.FechaRegistro >= DATEADD(day, -7, GETDATE())  -- Últimos 7 días
    -- r.TipoRegistro = 'Ingreso'  -- Solo ingresos

ORDER BY r.FechaRegistro DESC;

GO

-- ========================================
-- VERSIÓN SIMPLIFICADA (más rápida)
-- ========================================

SELECT 
    FORMAT(r.FechaRegistro, 'dd/MM/yyyy HH:mm:ss') AS FechaHora,
    r.TipoRegistro,
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN a.Nombre + ' ' + a.Apellido
        ELSE u.Nombre + ' ' + u.Apellido
    END AS Persona,
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN 'Aprendiz'
        ELSE 'Usuario'
    END AS Tipo,
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN a.NumeroDocumento
        ELSE u.NumeroDocumento
    END AS Documento,
    'Ver detalles' AS Dispositivos  -- Placeholder
FROM RegistroNFC r
LEFT JOIN Aprendiz a ON r.IdAprendiz = a.IdAprendiz
LEFT JOIN Usuario u ON r.IdUsuario = u.IdUsuario
WHERE r.FechaRegistro >= CAST(GETDATE() AS DATE)
ORDER BY r.FechaRegistro DESC;

GO

-- ========================================
-- CONSULTA CON TODOS LOS DETALLES
-- (Incluye columnas adicionales útiles)
-- ========================================

SELECT 
    -- ID del Registro
    r.IdRegistro,
    
    -- Fecha completa
    r.FechaRegistro,
    FORMAT(r.FechaRegistro, 'dd/MM/yyyy') AS Fecha,
    FORMAT(r.FechaRegistro, 'HH:mm:ss') AS Hora,
    DATENAME(weekday, r.FechaRegistro) AS DiaSemana,
    
    -- Tipo de registro
    r.TipoRegistro,
    r.Estado AS EstadoRegistro,
    
    -- Persona
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN 'Aprendiz'
        ELSE 'Usuario'
    END AS TipoPersona,
    
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN a.Nombre + ' ' + a.Apellido
        ELSE u.Nombre + ' ' + u.Apellido
    END AS NombreCompleto,
    
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN a.NumeroDocumento
        ELSE u.NumeroDocumento
    END AS NumeroDocumento,
    
    CASE 
        WHEN r.IdAprendiz IS NOT NULL THEN a.Correo
        ELSE u.Correo
    END AS Correo,
    
    -- Proceso asociado
    p.IdProceso,
    p.EstadoProceso,
    tp.Tipo AS TipoProceso,
    
    -- Dispositivos
    (
        SELECT STRING_AGG(
            te.Tipo + ' ' + e.Serial + 
            CASE WHEN ep.QuedoEnSena = 1 THEN ' (Quedó)' ELSE '' END,
            ', '
        )
        FROM ElementoProceso ep
        INNER JOIN Elemento e ON ep.IdElemento = e.IdElemento
        LEFT JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento
        WHERE ep.IdProceso = p.IdProceso AND ep.Validado = 1
    ) AS Dispositivos,
    
    -- Contadores
    (
        SELECT COUNT(*)
        FROM ElementoProceso ep
        WHERE ep.IdProceso = p.IdProceso
    ) AS TotalDispositivos,
    
    (
        SELECT COUNT(*)
        FROM ElementoProceso ep
        WHERE ep.IdProceso = p.IdProceso AND ep.QuedoEnSena = 1
    ) AS DispositivosQuedaron

FROM RegistroNFC r

LEFT JOIN Aprendiz a ON r.IdAprendiz = a.IdAprendiz
LEFT JOIN Usuario u ON r.IdUsuario = u.IdUsuario

-- Proceso más reciente
LEFT JOIN Proceso p ON p.IdProceso = (
    SELECT TOP 1 p2.IdProceso
    FROM Proceso p2
    WHERE 
        (
            (r.IdAprendiz IS NOT NULL AND p2.IdAprendiz = r.IdAprendiz AND p2.TipoPersona = 'Aprendiz') OR
            (r.IdUsuario IS NOT NULL AND p2.IdUsuario = r.IdUsuario AND p2.TipoPersona = 'Usuario')
        )
        AND p2.TimeStampEntradaSalida <= r.FechaRegistro
    ORDER BY p2.TimeStampEntradaSalida DESC
)

LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso

WHERE 
    r.FechaRegistro >= CAST(GETDATE() AS DATE)  -- Hoy

ORDER BY r.FechaRegistro DESC;
