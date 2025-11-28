using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_NFC.Data;             // DbContext (según tu proyecto)
using API___NFC.Models;        // DTO público
using API___NFC.Services.ExportHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Controllers
{
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

        // Método reutilizable que contiene tu lógica de negocio y devuelve la lista de DTOs
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
                {
                    query = query.Where(r => r.IdAprendiz != null);
                }
                else if (tp == "usuario")
                {
                    query = query.Where(r => r.IdUsuario != null);
                }
            }

            var registros = await query
                .OrderBy(r => r.FechaRegistro)
                .ToListAsync();

            var resultado = new List<FlujoNfcItemDto>();

            foreach (var r in registros)
            {
                var fechaRegistro = r.FechaRegistro ?? DateTime.MinValue;

                bool esAprendiz = r.IdAprendiz.HasValue;
                string tipoPer = esAprendiz ? "Aprendiz" : "Usuario";
                int idPersona = esAprendiz ? r.IdAprendiz!.Value : r.IdUsuario!.Value;

                string nombre = "";
                string apellido = "";
                string tipoDoc = "";
                string numDoc = "";

                if (esAprendiz)
                {
                    var apr = await _context.Aprendiz
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.IdAprendiz == idPersona);

                    if (apr != null)
                    {
                        nombre = apr.Nombre;
                        apellido = apr.Apellido;
                        tipoDoc = apr.TipoDocumento;
                        numDoc = apr.NumeroDocumento;
                    }
                }
                else
                {
                    var usr = await _context.Usuario
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.IdUsuario == idPersona);

                    if (usr != null)
                    {
                        nombre = usr.Nombre;
                        apellido = usr.Apellido;
                        tipoDoc = usr.TipoDocumento;
                        numDoc = usr.NumeroDocumento;
                    }
                }

                string nombreCompleto = $"{nombre} {apellido}".Trim();
                string documentoTexto = $"{tipoDoc} {numDoc}".Trim();

                var dispositivosLista = new List<string>();

                var proceso = await _context.Proceso
                    .AsNoTracking()
                    .Where(p =>
                        p.TipoPersona == tipoPer &&
                        ((esAprendiz && p.IdAprendiz == idPersona) ||
                         (!esAprendiz && p.IdUsuario == idPersona)) &&
                        p.TimeStampEntradaSalida <= fechaRegistro)
                    .OrderByDescending(p => p.TimeStampEntradaSalida)
                    .FirstOrDefaultAsync();

                if (proceso != null)
                {
                    var elementosProceso = await _context.ElementoProceso
                        .AsNoTracking()
                        .Include(ep => ep.Elemento)
                            .ThenInclude(e => e.TipoElemento)
                        .Where(ep => ep.IdProceso == proceso.IdProceso && ep.Validado == true)
                        .ToListAsync();

                    if (elementosProceso.Any())
                    {
                        foreach (var ep in elementosProceso)
                        {
                            var e = ep.Elemento;
                            if (e == null) continue;

                            var tipoE = e.TipoElemento?.Tipo ?? "Dispositivo";

                            var partesMM = new List<string>();
                            if (!string.IsNullOrWhiteSpace(e.Marca))
                                partesMM.Add(e.Marca);
                            if (!string.IsNullOrWhiteSpace(e.Modelo))
                                partesMM.Add(e.Modelo);

                            var mm = string.Join(" ", partesMM);
                            if (!string.IsNullOrWhiteSpace(mm))
                                mm = " " + mm;

                            var serial = string.IsNullOrWhiteSpace(e.Serial) ? "N/A" : e.Serial;

                            dispositivosLista.Add($"{tipoE}{mm} (S/N {serial})");
                        }
                    }
                }

                if (!dispositivosLista.Any())
                {
                    var dispositivosProp = await _context.Elemento
                        .AsNoTracking()
                        .Include(e => e.TipoElemento)
                        .Where(e => e.TipoPropietario == tipoPer && e.IdPropietario == idPersona)
                        .ToListAsync();

                    foreach (var e in dispositivosProp)
                    {
                        var tipoE = e.TipoElemento?.Tipo ?? "Dispositivo";

                        var partesMM = new List<string>();
                        if (!string.IsNullOrWhiteSpace(e.Marca))
                            partesMM.Add(e.Marca);
                        if (!string.IsNullOrWhiteSpace(e.Modelo))
                            partesMM.Add(e.Modelo);

                        var mm = string.Join(" ", partesMM);
                        if (!string.IsNullOrWhiteSpace(mm))
                            mm = " " + mm;

                        var serial = string.IsNullOrWhiteSpace(e.Serial) ? "N/A" : e.Serial;

                        dispositivosLista.Add($"{tipoE}{mm} (S/N {serial})");
                    }
                }

                var dto = new FlujoNfcItemDto
                {
                    IdRegistro = r.IdRegistro,
                    FechaRegistro = fechaRegistro,
                    TipoRegistro = r.TipoRegistro,
                    TipoPersona = tipoPer,
                    NombreCompleto = nombreCompleto,
                    Documento = documentoTexto,
                    DispositivosTexto = string.Join("; ", dispositivosLista)
                };

                resultado.Add(dto);
            }

            return resultado;
        }
    }
}