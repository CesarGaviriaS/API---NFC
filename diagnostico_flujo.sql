-- ========================================
-- DIAGNÓSTICO COMPLETO DE FLUJO NFC
-- ========================================

-- 1. Ver últimos RegistroNFC (lo que se guarda cuando pasas el tag)
SELECT 'ULTIMOS REGISTROS NFC' AS Seccion;
SELECT TOP 5
    r.IdRegistro,
    CONVERT(VARCHAR(20), r.FechaRegistro, 120) AS FechaHora,
    r.TipoRegistro,
    r.IdAprendiz,
    r.IdUsuario,
    a.Nombre + ' ' + a.Apellido AS NombreAprendiz
FROM RegistroNFC r
LEFT JOIN Aprendiz a ON r.IdAprendiz = a.IdAprendiz
ORDER BY r.IdRegistro DESC;

-- 2. Ver últimos Procesos (lo que se crea cuando haces ingreso/salida)
SELECT 'ULTIMOS PROCESOS' AS Seccion;
SELECT TOP 5
    p.IdProceso,
    CONVERT(VARCHAR(20), p.TimeStampEntradaSalida, 120) AS FechaHora,
    tp.Tipo AS TipoProceso,
    p.TipoPersona,
    p.IdAprendiz,
    p.IdUsuario,
    p.EstadoProceso,
    a.Nombre + ' ' + a.Apellido AS NombreAprendiz
FROM Proceso p
INNER JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
LEFT JOIN Aprendiz a ON p.IdAprendiz = a.IdAprendiz
ORDER BY p.IdProceso DESC;

-- 3. Ver dispositivos registrados en esos procesos
SELECT 'DISPOSITIVOS EN PROCESOS' AS Seccion;
SELECT TOP 10
    p.IdProceso,
    CONVERT(VARCHAR(20), p.TimeStampEntradaSalida, 120) AS FechaHoraProceso,
    tp.Tipo AS TipoProceso,
    ep.IdElementoProceso,
    ep.IdElemento,
    e.Serial,
    e.Marca,
    e.Modelo,
    te.Tipo AS TipoElemento,
    ep.Validado,
    ep.QuedoEnSena
FROM Proceso p
INNER JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
INNER JOIN ElementoProceso ep ON p.IdProceso = ep.IdProceso
INNER JOIN Elemento e ON ep.IdElemento = e.IdElemento
INNER JOIN TipoElemento te ON e.IdTipoElemento = te.IdTipoElemento
ORDER BY p.IdProceso DESC, ep.IdElementoProceso DESC;

-- 4. VERIFICACIÓN CRÍTICA: ¿Se están asociando correctamente RegistroNFC con Proceso?
SELECT 'ASOCIACION REGISTRO-PROCESO' AS Seccion;
SELECT 
    r.IdRegistro,
    CONVERT(VARCHAR(20), r.FechaRegistro, 120) AS FechaRegistro,
    r.TipoRegistro,
    p.IdProceso,
    CONVERT(VARCHAR(20), p.TimeStampEntradaSalida, 120) AS FechaProceso,
    DATEDIFF(SECOND, p.TimeStampEntradaSalida, r.FechaRegistro) AS DiferenciaSegundos,
    tp.Tipo,
    CASE 
        WHEN ep.IdElementoProceso IS NOT NULL THEN 'CON DISPOSITIVOS'
        ELSE 'SIN DISPOSITIVOS'
    END AS Estado
FROM (
    SELECT TOP 5 * FROM RegistroNFC ORDER BY IdRegistro DESC
) r
LEFT JOIN Proceso p ON 
    (r.IdAprendiz = p.IdAprendiz OR r.IdUsuario = p.IdUsuario) AND
    p.TipoPersona = CASE WHEN r.IdAprendiz IS NOT NULL THEN 'Aprendiz' ELSE 'Usuario' END AND
    ABS(DATEDIFF(SECOND, p.TimeStampEntradaSalida, r.FechaRegistro)) < 300
LEFT JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
LEFT JOIN ElementoProceso ep ON p.IdProceso = ep.IdProceso AND ep.Validado = 1
ORDER BY r.IdRegistro DESC;

-- 5. RESUMEN: Contar dispositivos por proceso para ver si se guardaron
SELECT 'RESUMEN POR PROCESO' AS Seccion;
SELECT TOP 5
    p.IdProceso,
    CONVERT(VARCHAR(20), p.TimeStampEntradaSalida, 120) AS Fecha,
    tp.Tipo AS TipoProceso,
    a.Nombre + ' ' + a.Apellido AS Persona,
    COUNT(ep.IdElementoProceso) AS TotalDispositivos,
    SUM(CASE WHEN ep.Validado = 1 THEN 1 ELSE 0 END) AS DispositivosValidados
FROM Proceso p
INNER JOIN TipoProceso tp ON p.IdTipoProceso = tp.IdTipoProceso
LEFT JOIN Aprendiz a ON p.IdAprendiz = a.IdAprendiz
LEFT JOIN ElementoProceso ep ON p.IdProceso = ep.IdProceso
GROUP BY p.IdProceso, p.TimeStampEntradaSalida, tp.Tipo, a.Nombre, a.Apellido
ORDER BY p.IdProceso DESC;
