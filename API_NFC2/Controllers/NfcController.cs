using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API_NFC.Data;
using API___NFC.Models;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador,Guardia")]
    public class NfcController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NfcController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Escanea un tag NFC:
        /// - Persona (1=Aprendiz, 2=Usuario): crea Proceso + RegistroNFC.
        ///   * Si no envías nombreTipoProceso, el backend alterna Entrada/Salida según el último registro.
        /// - Elemento (3=Elemento): asocia el elemento a un Proceso existente (requiere IdProceso).
        /// </summary>
        /// <remarks>
        /// Ejemplos:
        /// PERSONA (auto Entrada/Salida):
        /// {
        ///   "raw": "1,3",              // 1=Aprendiz, id=3
        ///   "idGuardia": 6
        /// }
        ///
        /// PERSONA (forzando tipo):
        /// {
        ///   "raw": "2,5",              // 2=Usuario, id=5
        ///   "idGuardia": 6,
        ///   "nombreTipoProceso": "Entrada"
        /// }
        ///
        /// ELEMENTO:
        /// {
        ///   "raw": "3,10",             // 3=Elemento, id=10
        ///   "idProceso": 123           // Proceso ya creado arriba (Entrada/Salida)
        /// }
        /// </remarks>
        [HttpPost("scan")]
        public async Task<IActionResult> Scan([FromBody] NfcScanRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Raw))
                    return BadRequest("El campo 'raw' es obligatorio.");

                var parts = req.Raw.Split(',');
                if (parts.Length != 2)
                    return BadRequest("Formato inválido en 'raw'. Usa 'tipo,id'.");

                if (!int.TryParse(parts[0], out int tipo) || !int.TryParse(parts[1], out int id))
                    return BadRequest("Formato de datos inválido en 'raw'.");

                // --- PERSONA (Aprendiz o Usuario) ---
                if (tipo == 1 || tipo == 2)
                {
                    // Cargar persona
                    object? persona = null;
                    string tipoPersona = (tipo == 1) ? "Aprendiz" : "Usuario";

                    if (tipo == 1)
                        persona = await _context.Aprendiz.AsNoTracking().FirstOrDefaultAsync(a => a.IdAprendiz == id);
                    else
                        persona = await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id);

                    if (persona == null)
                        return NotFound($"{tipoPersona} no encontrado.");

                    // Determinar Entrada/Salida
                    string nombreTipoProceso = req.NombreTipoProceso;
                    if (string.IsNullOrWhiteSpace(nombreTipoProceso))
                    {
                        // Último registro NFC de esta persona
                        var ultimo = await _context.RegistroNFC
                            .Where(r => (tipo == 1 ? r.IdAprendiz == id : r.IdUsuario == id))
                            .OrderByDescending(r => r.FechaRegistro)
                            .FirstOrDefaultAsync();

                        // Regla: si último fue Entrada -> ahora Salida; si no hay o fue Salida -> Entrada
                        if (ultimo == null || string.Equals(ultimo.TipoRegistro, "Salida", StringComparison.OrdinalIgnoreCase))
                            nombreTipoProceso = "Entrada";
                        else
                            nombreTipoProceso = "Salida";
                    }

                    // (OPCIONAL) Validación estricta: bloquear duplicado consecutivo
                    /*
                    var ultimoStrict = await _context.RegistroNFC
                        .Where(r => (tipo == 1 ? r.IdAprendiz == id : r.IdUsuario == id))
                        .OrderByDescending(r => r.FechaRegistro)
                        .FirstOrDefaultAsync();

                    if (ultimoStrict != null &&
                        string.Equals(ultimoStrict.TipoRegistro, nombreTipoProceso, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(req.NombreTipoProceso)) // solo si lo forzaron desde el cliente
                    {
                        return BadRequest($"Ya existe una '{nombreTipoProceso}' anterior; realiza la operación opuesta.");
                    }
                    */

                    // Buscar tipo de proceso en catálogo
                    var tipoProc = await _context.TipoProceso
                        .FirstOrDefaultAsync(t => t.Tipo == nombreTipoProceso);

                    if (tipoProc == null)
                        return NotFound($"No existe un TipoProceso llamado '{nombreTipoProceso}'.");

                    // Crear Proceso
                    var proceso = new Proceso
                    {
                        IdTipoProceso = tipoProc.IdTipoProceso,
                        TipoPersona = tipoPersona,                 // "Aprendiz" o "Usuario"
                        IdGuardia = req.IdGuardia ?? 0,           // Si no envían, 0
                        TimeStampEntradaSalida = DateTime.Now,
                        RequiereOtrosProcesos = false,
                        Observaciones = $"{nombreTipoProceso} registrada mediante NFC",
                        SincronizadoBD = true,
                        IdAprendiz = (tipo == 1) ? (int?)id : null,
                        IdUsuario = (tipo == 2) ? (int?)id : null
                    };

                    _context.Proceso.Add(proceso);
                    await _context.SaveChangesAsync();

                    // Registrar NFC
                    var registro = new RegistroNFC
                    {
                        IdAprendiz = (tipo == 1) ? id : null,
                        IdUsuario = (tipo == 2) ? id : null,
                        TipoRegistro = nombreTipoProceso,
                        FechaRegistro = DateTime.Now,
                        Estado = "Activo"
                    };

                    _context.RegistroNFC.Add(registro);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        Message = "✅ Proceso y Registro NFC creados correctamente.",
                        TipoPersona = tipoPersona,
                        Proceso = proceso,
                        Registro = registro
                    });
                }

                // --- ELEMENTO ---
                if (tipo == 3)
                {
                    if (req.IdProceso == null || req.IdProceso == 0)
                        return BadRequest("Debe enviar 'idProceso' para asociar el elemento.");

                    var elemento = await _context.Elemento.AsNoTracking().FirstOrDefaultAsync(e => e.IdElemento == id);
                    if (elemento == null)
                        return NotFound("Elemento no encontrado.");

                    var proceso = await _context.Proceso.FirstOrDefaultAsync(p => p.IdProceso == req.IdProceso);
                    if (proceso == null)
                        return NotFound("Proceso no encontrado.");

                    var elementoProc = new ElementoProceso
                    {
                        IdElemento = id,
                        IdProceso = proceso.IdProceso,
                        Validado = true
                    };

                    _context.ElementoProceso.Add(elementoProc);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        Message = "✅ Elemento asociado correctamente al proceso.",
                        ElementoProceso = elementoProc
                    });
                }

                return BadRequest("Tipo inválido. Use 1=Aprendiz, 2=Usuario, 3=Elemento.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }

    public class NfcScanRequest
    {
        public string Raw { get; set; } = string.Empty;  // "tipo,id"
        public int? IdGuardia { get; set; }              // Id del guardia que ejecuta el escaneo
        public string? NombreTipoProceso { get; set; }   // "Entrada" | "Salida" (opcional: si no viene, se auto-resuelve)
        public int? IdProceso { get; set; }              // Para tipo==3 (elemento)
    }
}
