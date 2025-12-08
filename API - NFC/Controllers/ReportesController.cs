using API___NFC.Models;
using API___NFC.Services.ExportHelpers;
using API_NFC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ReportesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Flujo de ingresos/salidas con persona + dispositivos.
    /// GET: api/Reportes/FlujoNFC
    /// </summary>
    [HttpGet("FlujoNFC")]
    public async Task<ActionResult<IEnumerable<FlujoNfcItemDto>>> GetFlujoNfc(
        DateTime? desde = null,
        DateTime? hasta = null,
        string? tipoRegistro = null,
        string? tipoPersona = null)
    {
        var resultado = await BuildFlujoAsync(desde, hasta, tipoRegistro, tipoPersona);
        return Ok(resultado);
    }

    // Endpoint para exportar PDF
    [HttpGet("FlujoNFC/export/pdf")]
    public async Task<IActionResult> ExportFlujoPdf(
        DateTime? desde = null,
        DateTime? hasta = null,
        string? tipoRegistro = null,
        string? tipoPersona = null)
    {
        var datos = await BuildFlujoAsync(desde, hasta, tipoRegistro, tipoPersona);
        var pdfBytes = PdfExporter.GeneratePdf(datos);
        var fileName = $"flujo_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // Endpoint para exportar Word (DOCX)
    [HttpGet("FlujoNFC/export/word")]
    public async Task<IActionResult> ExportFlujoWord(
        DateTime? desde = null,
        DateTime? hasta = null,
        string? tipoRegistro = null,
        string? tipoPersona = null)
    {
        var datos = await BuildFlujoAsync(desde, hasta, tipoRegistro, tipoPersona);
        var docBytes = WordExporter.GenerateWord(datos);
        var fileName = $"flujo_{DateTime.Now:yyyyMMddHHmmss}.docx";
        return File(docBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }

    // Método reutilizable optimizado - 4 queries en lugar de 400+
    private async Task<List<FlujoNfcItemDto>> BuildFlujoAsync(
 DateTime? desde = null,
 DateTime? hasta = null,
 string? tipoRegistro = null,
 string? tipoPersona = null)
    {
        // Normalizar rango de fechas
        if (desde == null && hasta == null)
        {
            var hoy = DateTime.Today;
            desde = hoy;
            hasta = hoy.AddDays(1).AddTicks(-1);
        }
        else
        {
            if (desde != null)
                desde = desde.Value.Date;

            if (hasta != null)
                hasta = hasta.Value.Date.AddDays(1).AddTicks(-1);
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n📊 ═══════════════════════════════════════");
        Console.WriteLine($"📊 GENERANDO REPORTE DE FLUJO");
        Console.WriteLine($"📊 Desde: {desde:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"📊 Hasta: {hasta:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"📊 ═══════════════════════════════════════");
        Console.ResetColor();

        // ✅ QUERY 1: Obtener registros base
        var query = _context.RegistroNFC
            .AsNoTracking()
            .Where(r => r.FechaRegistro >= desde && r.FechaRegistro <= hasta);

        if (!string.IsNullOrWhiteSpace(tipoRegistro))
        {
            var tr = tipoRegistro.Trim();
            query = query.Where(r => r.TipoRegistro == tr);
        }

        if (!string.IsNullOrWhiteSpace(tipoPersona))
        {
            var tp = tipoPersona.Trim().ToLower();
            if (tp == "aprendiz")
                query = query.Where(r => r.IdAprendiz != null);
            else if (tp == "usuario")
                query = query.Where(r => r.IdUsuario != null);
        }

        var registros = await query.OrderBy(r => r.FechaRegistro).ToListAsync();

        Console.WriteLine($"📋 Total de registros encontrados: {registros.Count}");

        if (!registros.Any())
            return new List<FlujoNfcItemDto>();

        // ✅ QUERY 2: Obtener aprendices
        var idsAprendices = registros
            .Where(r => r.IdAprendiz.HasValue)
            .Select(r => r.IdAprendiz.Value)
            .Distinct()
            .ToList();

        var aprendices = await _context.Aprendiz
            .AsNoTracking()
            .Where(a => idsAprendices.Contains(a.IdAprendiz))
            .ToDictionaryAsync(a => a.IdAprendiz);

        // ✅ QUERY 3: Obtener usuarios
        var idsUsuarios = registros
            .Where(r => r.IdUsuario.HasValue)
            .Select(r => r.IdUsuario.Value)
            .Distinct()
            .ToList();

        var usuarios = await _context.Usuario
            .AsNoTracking()
            .Where(u => idsUsuarios.Contains(u.IdUsuario))
            .ToDictionaryAsync(u => u.IdUsuario);

        // ✅ QUERY 4: Obtener procesos CON dispositivos
        // ✅ QUERY 4: Obtener procesos en el rango de fechas
        var procesos = await _context.Proceso
            .AsNoTracking()
            .Include(p => p.TipoProceso)
            .Where(p =>
                p.TimeStampEntradaSalida >= desde &&
                p.TimeStampEntradaSalida <= hasta)
            // ✅ REMOVIDO: Ya no filtramos por "Cerrado"
            // Ahora mostramos TODOS los estados: Abierto, EnCurso, Cerrado
            .ToListAsync();

        Console.WriteLine($"📦 Total de procesos encontrados: {procesos.Count}");

        var idsProcesos = procesos.Select(p => p.IdProceso).ToList();

        // ✅ QUERY 5: Obtener DetalleRegistroNFC de esos procesos (NUEVA TABLA)
        var detallesRegistro = await _context.DetalleRegistroNFC
            .AsNoTracking()
            .Include(d => d.Elemento)
                .ThenInclude(e => e.TipoElemento)
            .Where(d => idsProcesos.Contains(d.IdProceso))
            .ToListAsync();

        Console.WriteLine($"📱 Total de DetalleRegistroNFC encontrados: {detallesRegistro.Count}");

        // Agrupar DetalleRegistroNFC por IdProceso
        var detallesPorProceso = detallesRegistro
            .GroupBy(d => d.IdProceso)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ✅ Procesar resultados
        var resultado = new List<FlujoNfcItemDto>();

        foreach (var r in registros)
        {
            var fechaRegistro = r.FechaRegistro ?? DateTime.MinValue;
            bool esAprendiz = r.IdAprendiz.HasValue;
            int idPersona = esAprendiz ? r.IdAprendiz!.Value : r.IdUsuario!.Value;
            string tipoPer = esAprendiz ? "Aprendiz" : "Usuario";

            Console.WriteLine($"\n🔍 Procesando registro {r.IdRegistro}:");
            Console.WriteLine($"   • Tipo: {r.TipoRegistro}");
            Console.WriteLine($"   • Persona: {tipoPer} ID {idPersona}");
            Console.WriteLine($"   • Fecha: {fechaRegistro:dd/MM/yyyy HH:mm:ss}");

            // Datos de persona
            string nombre = "", apellido = "", tipoDoc = "", numDoc = "";

            if (esAprendiz && aprendices.TryGetValue(idPersona, out var apr))
            {
                nombre = apr.Nombre ?? "";
                apellido = apr.Apellido ?? "";
                tipoDoc = apr.TipoDocumento ?? "";
                numDoc = apr.NumeroDocumento ?? "";
            }
            else if (!esAprendiz && usuarios.TryGetValue(idPersona, out var usr))
            {
                nombre = usr.Nombre ?? "";
                apellido = usr.Apellido ?? "";
                tipoDoc = usr.TipoDocumento ?? "";
                numDoc = usr.NumeroDocumento ?? "";
            }

            string nombreCompleto = $"{nombre} {apellido}".Trim();
            string documentoTexto = $"{tipoDoc} {numDoc}".Trim();

            // ✅ BUSCAR PROCESO DIRECTAMENTE POR IdProceso (mucho más confiable)
            Proceso? procesoMatch = null;

            if (r.IdProceso.HasValue)
            {
                // Uso directo del FK IdProceso - más preciso
                procesoMatch = procesos.FirstOrDefault(p => p.IdProceso == r.IdProceso.Value);
            }
            else
            {
                // Fallback para registros viejos sin IdProceso (búsqueda por tiempo)
                var tolerancia = TimeSpan.FromMinutes(5);
                procesoMatch = procesos
                    .Where(p =>
                        p.TimeStampEntradaSalida.HasValue &&
                        p.TipoPersona == tipoPer &&
                        ((esAprendiz && p.IdAprendiz == idPersona) || (!esAprendiz && p.IdUsuario == idPersona)) &&
                        p.TipoProceso != null &&
                        p.TipoProceso.Tipo == r.TipoRegistro &&
                        Math.Abs((p.TimeStampEntradaSalida.Value - fechaRegistro).TotalSeconds) < tolerancia.TotalSeconds)
                    .OrderBy(p => Math.Abs((p.TimeStampEntradaSalida!.Value - fechaRegistro).TotalSeconds))
                    .FirstOrDefault();
            }

            // Construir texto de dispositivos
            var dispositivosTexto = "Sin dispositivos";

            if (procesoMatch != null)
            {
                Console.WriteLine($"   ✅ Proceso encontrado: {procesoMatch.IdProceso}");

                if (detallesPorProceso.TryGetValue(procesoMatch.IdProceso, out var dispositivosDelProceso))
                {
                    Console.WriteLine($"   📱 Dispositivos en el proceso: {dispositivosDelProceso.Count}");

                    List<DetalleRegistroNFC> dispositivosFiltrados;

                    if (r.TipoRegistro == "Ingreso")
                    {
                        // ✅ INGRESO: Mostrar TODOS los dispositivos con Accion='Ingreso'
                        dispositivosFiltrados = dispositivosDelProceso
                            .Where(d => d.Accion == "Ingreso")
                            .ToList();

                        Console.WriteLine($"   🟢 INGRESO → Mostrando {dispositivosFiltrados.Count} dispositivos ingresados");
                    }
                    else
                    {
                        // ✅ SALIDA: Mostrar los que SALIERON (Accion='Salida')
                        // Los que "Quedaron" NO se muestran
                        dispositivosFiltrados = dispositivosDelProceso
                            .Where(d => d.Accion == "Salida")
                            .ToList();

                        var quedaron = dispositivosDelProceso
                            .Count(d => d.Accion == "Quedo");

                        Console.WriteLine($"   🟠 SALIDA → {dispositivosFiltrados.Count} salieron, {quedaron} quedaron en SENA");
                    }

                    var dispositivosLista = dispositivosFiltrados
                        .Select(d =>
                        {
                            var e = d.Elemento;
                            if (e == null) return null;

                            var tipoE = e.TipoElemento?.Tipo ?? "Dispositivo";
                            var partesMM = new[] { e.Marca, e.Modelo }
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToList();
                            var mm = partesMM.Any() ? " " + string.Join(" ", partesMM) : "";
                            var serial = string.IsNullOrWhiteSpace(e.Serial) ? "N/A" : e.Serial;

                            return $"{tipoE}{mm} (S/N {serial})";
                        })
                        .Where(x => x != null)
                        .ToList();

                    if (dispositivosLista.Any())
                    {
                        dispositivosTexto = string.Join("; ", dispositivosLista);
                        Console.WriteLine($"   ✅ Texto generado: {dispositivosTexto}");
                    }
                    else
                    {
                        Console.WriteLine($"   ⚠️ No hay dispositivos para mostrar");
                    }
                }
                else
                {
                    Console.WriteLine($"   ⚠️ No se encontraron dispositivos para el proceso");
                }
            }
            else
            {
                Console.WriteLine($"   ❌ No se encontró proceso que coincida");
            }

            resultado.Add(new FlujoNfcItemDto
            {
                IdRegistro = r.IdRegistro,
                FechaRegistro = fechaRegistro,
                TipoRegistro = r.TipoRegistro ?? "",
                TipoPersona = tipoPer,
                NombreCompleto = nombreCompleto,
                Documento = documentoTexto,
                DispositivosTexto = dispositivosTexto
            });
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n✅ Reporte generado: {resultado.Count} registros");
        Console.WriteLine($"═══════════════════════════════════════\n");
        Console.ResetColor();

        return resultado;
    }
}
