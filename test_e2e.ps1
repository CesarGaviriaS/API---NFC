# Script de Prueba End-to-End del Sistema NFC
# Este script simula un flujo completo de ingreso y salida

$baseUrl = "http://localhost:5075"
$aprendizId = 7  # yanfa perez
$dispositivo1 = 16  # Serial: 123H
$dispositivo2 = 17  # Serial: 123P

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "PRUEBA END-TO-END - Sistema NFC" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# PASO 1: Verificar estado inicial
Write-Host "[1] Verificando estado inicial..." -ForegroundColor Yellow
$registrosAntes = sqlcmd -S localhost -d NFCSENA -Q "SELECT COUNT(*) AS Total FROM RegistroNFC" -h -1 -W | Select-String -Pattern '\d+' | ForEach-Object { $_.Matches.Value }
Write-Host "   Registros NFC actuales: $registrosAntes`n"

# PASO 2: Crear proceso de INGRESO
Write-Host "[2] Creando proceso de INGRESO..." -ForegroundColor Yellow
$ingresoBody = @{
    IdTipoProceso = 1  # Asumiendo que 1 es Ingreso
    TipoPersona = "Aprendiz"
    IdAprendiz = $aprendizId
    IdGuardia = 1
    dispositivos = @($dispositivo1, $dispositivo2)
} | ConvertTo-Json

try {
    $responseIngreso = Invoke-RestMethod -Uri "$baseUrl/api/Procesoes/crear" `
        -Method POST `
        -Body $ingresoBody `
        -ContentType "application/json"
    
    $procesoIngresoId = $responseIngreso.idProceso
    Write-Host "   ✅ Proceso de ingreso creado: ID $procesoIngresoId`n" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Error creando ingreso: $_" -ForegroundColor Red
    exit 1
}

# PASO 3: Verificar en base de datos
Start-Sleep -Seconds 2
Write-Host "[3] Verificando datos en BD..." -ForegroundColor Yellow
sqlcmd -S localhost -d NFCSENA -Q "
    SELECT 'Proceso creado:' AS Info; 
    SELECT IdProceso, EstadoProceso, TimeStampEntradaSalida FROM Proceso WHERE IdProceso = $procesoIngresoId;
    
    SELECT 'ElementoProceso registrados:' AS Info;
    SELECT ep.IdElementoProceso, ep.IdElemento, e.Serial, ep.Validado 
    FROM ElementoProceso ep 
    INNER JOIN Elemento e ON ep.IdElemento = e.IdElemento 
    WHERE ep.IdProceso = $procesoIngresoId;
"

# PASO 4: Confirmar INGRESO
Write-Host "`n[4] Confirmando INGRESO..." -ForegroundColor Yellow
try {
    $responseConfirmarIngreso = Invoke-RestMethod -Uri "$baseUrl/api/Procesoes/confirmarIngreso/$procesoIngresoId" `
        -Method POST `
        -ContentType "application/json"
    
    Write-Host "   ✅ Ingreso confirmado" -ForegroundColor Green
    Write-Host "   Estado: $($responseConfirmarIngreso.estadoNuevo)" -ForegroundColor Cyan
    Write-Host "   Dispositivos registrados: $($responseConfirmarIngreso.dispositivosRegistrados)`n"
} catch {
    Write-Host "   ❌ Error confirmando ingreso: $_" -ForegroundColor Red
}

# PASO 5: Verificar RegistroNFC
Write-Host "[5] Verificando RegistroNFC..." -ForegroundColor Yellow
$ultimoRegistroIngreso = sqlcmd -S localhost -d NFCSENA -Q "
    SELECT TOP 1 IdRegistro, FechaRegistro, TipoRegistro, IdAprendiz 
    FROM RegistroNFC 
    ORDER BY IdRegistro DESC
" -h -1 -W
Write-Host "$ultimoRegistroIngreso`n"

# ESPERAR UN MOMENTO
Write-Host "[ESPERA] Pausando 3 segundos antes de salida...`n" -ForegroundColor Gray
Start-Sleep -Seconds 3

# PASO 6: Crear proceso de SALIDA
Write-Host "[6] Creando proceso de SALIDA..." -ForegroundColor Yellow
$salidaBody = @{
    IdTipoProceso = 2  # Asumiendo que 2 es Salida
    TipoPersona = "Aprendiz"
    IdAprendiz = $aprendizId
    IdGuardia = 1
    dispositivos = @($dispositivo1)  # Solo saca el dispositivo 1, el 2 queda
} | ConvertTo-Json

try {
    $responseSalida = Invoke-RestMethod -Uri "$baseUrl/api/Procesoes/crear" `
        -Method POST `
        -Body $salidaBody `
        -ContentType "application/json"
    
    $procesoSalidaId = $responseSalida.idProceso
    Write-Host "   ✅ Proceso de salida creado: ID $procesoSalidaId`n" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Error creando salida: $_" -ForegroundColor Red
    exit 1
}

# PASO 7: Marcar dispositivo 2 como "Queda en SENA"
Write-Host "[7] Marcando dispositivo 17 (123P) como QuedoEnSena..." -ForegroundColor Yellow
$epId = sqlcmd -S localhost -d NFCSENA -Q "SELECT IdElementoProceso FROM ElementoProceso WHERE IdProceso = $procesoSalidaId AND IdElemento = $dispositivo2" -h -1 -W | Select-String -Pattern '\d+' | ForEach-Object { $_.Matches.Value }

if ($epId) {
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/ElementoProcesoes/marcarQuedoSena/$epId" `
            -Method PUT `
            -ContentType "application/json"
        Write-Host "   ✅ Dispositivo 17 marcado como QuedoEnSena`n" -ForegroundColor Green
    } catch {
        Write-Host "   ⚠️ No se pudo marcar QuedoEnSena (puede que no esté en el proceso)`n" -ForegroundColor Yellow
    }
}

# PASO 8: Confirmar SALIDA
Write-Host "[8] Confirmando SALIDA..." -ForegroundColor Yellow
try {
    $responseConfirmarSalida = Invoke-RestMethod -Uri "$baseUrl/api/Procesoes/confirmarSalida/$procesoSalidaId" `
        -Method POST `
        -ContentType "application/json"
    
    Write-Host "   ✅ Salida confirmada" -ForegroundColor Green
    Write-Host "   Dispositivos que salieron: $($responseConfirmarSalida.dispositivosSalieron)" -ForegroundColor Cyan
    Write-Host "   Dispositivos que quedaron: $($responseConfirmarSalida.dispositivosQuedaron)" -ForegroundColor Cyan
    Write-Host "   Pendientes liberados: $($responseConfirmarSalida.dispositivosPendientesLiberados)`n" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ Error confirmando salida: $_`n" -ForegroundColor Red
}

# PASO 9: Verificar RegistroNFC final
Write-Host "[9] Verificando registros finales en BD..." -ForegroundColor Yellow
sqlcmd -S localhost -d NFCSENA -Q "
    SELECT TOP 5 
        r.IdRegistro,
        CONVERT(VARCHAR(20), r.FechaRegistro, 120) AS FechaHora,
        r.TipoRegistro,
        a.Nombre + ' ' + a.Apellido AS Persona
    FROM RegistroNFC r
    LEFT JOIN Aprendiz a ON r.IdAprendiz = a.IdAprendiz
    ORDER BY r.IdRegistro DESC
"

# PASO 10: Verificar FlujoNFC endpoint
Write-Host "`n[10] Verificando endpoint FlujoNFC..." -ForegroundColor Yellow
$fechaHoy = (Get-Date).ToString("yyyy-MM-ddT00:00:00")
$fechaManana = (Get-Date).AddDays(1).ToString("yyyy-MM-ddT00:00:00")

try {
    $flujoData = Invoke-RestMethod -Uri "$baseUrl/api/Reportes/FlujoNFC?desde=$fechaHoy&hasta=$fechaManana" -Method GET
    
    $ultimosDos = $flujoData | Select-Object -Last 2
    
    Write-Host "`n   📊 Últimos 2 registros en FlujoNFC:" -ForegroundColor Cyan
    foreach ($item in $ultimosDos) {
        Write-Host "   -----------------------------------" -ForegroundColor Gray
        Write-Host "   Registro: $($item.idRegistro)" -ForegroundColor White
        Write-Host "   Fecha: $($item.fechaRegistro)" -ForegroundColor White
        Write-Host "   Tipo: $($item.tipoRegistro)" -ForegroundColor White
        Write-Host "   Persona: $($item.nombreCompleto)" -ForegroundColor White
        Write-Host "   Dispositivos: $($item.dispositivosTexto)" -ForegroundColor Yellow
    }
    
    Write-Host "`n   ✅ Endpoint FlujoNFC funciona correctamente`n" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Error consultando FlujoNFC: $_`n" -ForegroundColor Red
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PRUEBA COMPLETADA" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan
