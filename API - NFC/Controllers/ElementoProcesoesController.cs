using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_NFC.Data;
using API___NFC.Models;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElementoProcesoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ElementoProcesoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetElementoProcesos()
        {
            var elementoProcesos = await _context.ElementoProceso
                .Include(e => e.Elemento).ThenInclude(e => e.TipoElemento)
                .Include(e => e.Proceso)
                .AsNoTracking()
                .Select(ep => new
                {
                    ep.IdElementoProceso,
                    ep.IdElemento,
                    ep.IdProceso,
                    ep.Validado,
                    ep.QuedoEnSena,
                    Elemento = ep.Elemento != null ? new
                    {
                        ep.Elemento.IdElemento,
                        ep.Elemento.Marca,
                        ep.Elemento.Modelo,
                        ep.Elemento.Serial,
                        ep.Elemento.Descripcion,
                        ep.Elemento.ImagenUrl,
                        ep.Elemento.CodigoNFC,
                        TipoElemento = ep.Elemento.TipoElemento != null ? new
                        {
                            ep.Elemento.TipoElemento.IdTipoElemento,
                            ep.Elemento.TipoElemento.Tipo
                        } : null
                    } : null
                })
                .ToListAsync();

            return Ok(elementoProcesos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetElementoProceso(int id)
        {
            var elementoProceso = await _context.ElementoProceso
                .Include(e => e.Elemento).ThenInclude(e => e.TipoElemento)
                .Include(e => e.Proceso)
                .AsNoTracking()
                .Where(ep => ep.IdElementoProceso == id)
                .Select(ep => new
                {
                    ep.IdElementoProceso,
                    ep.IdElemento,
                    ep.IdProceso,
                    ep.Validado,
                    ep.QuedoEnSena,
                    Elemento = ep.Elemento != null ? new
                    {
                        ep.Elemento.IdElemento,
                        ep.Elemento.Marca,
                        ep.Elemento.Modelo,
                        ep.Elemento.Serial,
                        ep.Elemento.Descripcion,
                        ep.Elemento.ImagenUrl,
                        ep.Elemento.CodigoNFC,
                        TipoElemento = ep.Elemento.TipoElemento != null ? new
                        {
                            ep.Elemento.TipoElemento.IdTipoElemento,
                            ep.Elemento.TipoElemento.Tipo
                        } : null
                    } : null
                })
                .FirstOrDefaultAsync();

            if (elementoProceso == null)
                return NotFound(new { Message = "ElementoProceso no encontrado." });

            return Ok(elementoProceso);
        }

        [HttpGet("byProceso/{idProceso}")]
        public async Task<ActionResult<IEnumerable<object>>> GetByProceso(int idProceso)
        {
            var relaciones = await _context.ElementoProceso
                .Where(e => e.IdProceso == idProceso)
                .Include(e => e.Elemento).ThenInclude(e => e.TipoElemento)
                .AsNoTracking()
                .Select(ep => new
                {
                    ep.IdElementoProceso,
                    ep.IdElemento,
                    ep.IdProceso,
                    ep.Validado,
                    ep.QuedoEnSena,
                    Elemento = ep.Elemento != null ? new
                    {
                        ep.Elemento.IdElemento,
                        ep.Elemento.Marca,
                        ep.Elemento.Modelo,
                        ep.Elemento.Serial,
                        ep.Elemento.Descripcion,
                        ep.Elemento.ImagenUrl,
                        ep.Elemento.CodigoNFC,
                        ep.Elemento.Estado,
                        TipoElemento = ep.Elemento.TipoElemento != null ? new
                        {
                            ep.Elemento.TipoElemento.IdTipoElemento,
                            ep.Elemento.TipoElemento.Tipo,
                            ep.Elemento.TipoElemento.RequiereNFC
                        } : null
                    } : null
                })
                .ToListAsync();

            return Ok(relaciones);
        }

        // ✅ MÉTODO CLAVE: Obtener dispositivos pendientes (QuedoEnSena = true)
        [HttpGet("pendientes/{tipoPropietario}/{idPropietario}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendientes(string tipoPropietario, int idPropietario)
        {
            var pendientes = await _context.ElementoProceso
                .Include(e => e.Elemento).ThenInclude(e => e.TipoElemento)
                .Where(e =>
                    e.Elemento.IdPropietario == idPropietario &&
                    e.Elemento.TipoPropietario == tipoPropietario &&
                    e.QuedoEnSena == true)
                .Select(ep => new
                {
                    ep.IdElementoProceso,
                    ep.IdProceso,
                    ep.IdElemento,
                    ep.QuedoEnSena,
                    ep.Validado,
                    Elemento = new
                    {
                        ep.Elemento.IdElemento,
                        ep.Elemento.Marca,
                        ep.Elemento.Modelo,
                        ep.Elemento.Serial,
                        ep.Elemento.Descripcion,
                        ep.Elemento.ImagenUrl,
                        ep.Elemento.CodigoNFC,
                        TipoElemento = new
                        {
                            ep.Elemento.TipoElemento.IdTipoElemento,
                            ep.Elemento.TipoElemento.Tipo
                        }
                    }
                })
                .ToListAsync();

            return Ok(pendientes);
        }

        // ✅ NUEVO: Agregar automáticamente pendientes en SALIDA
        [HttpPost("agregarPendientesASalida")]
        public async Task<ActionResult> AgregarPendientesASalida([FromBody] AgregarPendientesRequest request)
        {
            try
            {
                // Obtener dispositivos pendientes
                var pendientes = await _context.ElementoProceso
                    .Include(e => e.Elemento)
                    .Where(ep =>
                        ep.Elemento.IdPropietario == request.IdPropietario &&
                        ep.Elemento.TipoPropietario == request.TipoPropietario &&
                        ep.QuedoEnSena == true)
                    .ToListAsync();

                if (pendientes.Count == 0)
                {
                    return Ok(new { Message = "No hay dispositivos pendientes", Agregados = 0 });
                }

                int agregados = 0;

                foreach (var pendiente in pendientes)
                {
                    // Cambiar el proceso al actual y marcar como no pendiente
                    pendiente.IdProceso = request.IdProcesoSalida;
                    pendiente.QuedoEnSena = false;
                    pendiente.Validado = true;
                    agregados++;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Dispositivos pendientes agregados a la salida",
                    Agregados = agregados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error al agregar pendientes",
                    Error = ex.Message
                });
            }
        }

        // ✅ MODIFICADO: Auto-registrar SIN incluir pendientes
        [HttpPost("autoRegister")]
        public async Task<ActionResult<object>> AutoRegisterDevices([FromBody] AutoRegisterRequest request)
        {
            try
            {
                var proceso = await _context.Proceso.FindAsync(request.IdProceso);
                if (proceso == null)
                    return NotFound(new { Message = "El proceso no existe." });

                // Obtener todos los dispositivos del usuario
                var dispositivos = await _context.Elemento
                    .Where(e =>
                        e.IdPropietario == request.IdPropietario &&
                        e.TipoPropietario == request.TipoPropietario &&
                        e.Estado == true)
                    .ToListAsync();

                // Obtener IDs de dispositivos pendientes (QuedoEnSena = true)
                var pendientesIds = await _context.ElementoProceso
                    .Where(ep =>
                        ep.Elemento.IdPropietario == request.IdPropietario &&
                        ep.Elemento.TipoPropietario == request.TipoPropietario &&
                        ep.QuedoEnSena == true)
                    .Select(ep => ep.IdElemento)
                    .ToListAsync();

                int registrados = 0, omitidos = 0, omitidosPendientes = 0;

                foreach (var dispositivo in dispositivos)
                {
                    // ❌ NO registrar si está pendiente (QuedoEnSena = true)
                    if (pendientesIds.Contains(dispositivo.IdElemento))
                    {
                        omitidosPendientes++;
                        continue;
                    }

                    // Verificar si ya existe en este proceso
                    bool yaExiste = await _context.ElementoProceso
                        .AnyAsync(ep => ep.IdElemento == dispositivo.IdElemento &&
                                        ep.IdProceso == request.IdProceso);

                    if (yaExiste)
                    {
                        omitidos++;
                        continue;
                    }

                    // Registrar dispositivo
                    var elementoProceso = new ElementoProceso
                    {
                        IdElemento = dispositivo.IdElemento,
                        IdProceso = request.IdProceso,
                        Validado = true,
                        QuedoEnSena = false
                    };

                    _context.ElementoProceso.Add(elementoProceso);
                    registrados++;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Auto-registro completado",
                    Registrados = registrados,
                    Omitidos = omitidos,
                    OmitidosPendientes = omitidosPendientes,
                    Total = dispositivos.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error en auto-registro",
                    Error = ex.Message
                });
            }
        }

        // ✅ MODIFICADO: Marcar como "Quedó en SENA"
        [HttpPost("marcarQuedoSena/{id}")]
        public async Task<ActionResult> MarcarQuedoSena(int id)
        {
            try
            {
                var elementoProceso = await _context.ElementoProceso
                    .Include(ep => ep.Elemento)
                    .FirstOrDefaultAsync(ep => ep.IdElementoProceso == id);

                if (elementoProceso == null)
                    return NotFound(new { Message = "Relación no encontrada." });

                // Marcar como pendiente
                elementoProceso.QuedoEnSena = true;
                elementoProceso.Validado = true;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "El dispositivo permanece en las instalaciones del Centro de Formación CIMM.'",
                    IdElementoProceso = elementoProceso.IdElementoProceso,
                    IdElemento = elementoProceso.IdElemento,
                    QuedoEnSena = elementoProceso.QuedoEnSena
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error al marcar dispositivo",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutElementoProceso(int id, [FromBody] ElementoProcesoUpdateDto dto)
        {
            try
            {
                var existing = await _context.ElementoProceso.FindAsync(id);
                if (existing == null)
                    return NotFound(new { Message = "ElementoProceso no encontrado." });

                if (dto.QuedoEnSena.HasValue)
                    existing.QuedoEnSena = dto.QuedoEnSena.Value;

                if (dto.Validado.HasValue)
                    existing.Validado = dto.Validado.Value;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error interno del servidor",
                    Details = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ElementoProceso>> PostElementoProceso([FromBody] ElementoProceso elementoProceso)
        {
            if (!await _context.Elemento.AnyAsync(e => e.IdElemento == elementoProceso.IdElemento))
                return BadRequest(new { Message = "El elemento asociado no existe." });

            if (!await _context.Proceso.AnyAsync(p => p.IdProceso == elementoProceso.IdProceso))
                return BadRequest(new { Message = "El proceso asociado no existe." });

            // Verificar duplicado en este proceso
            if (await _context.ElementoProceso.AnyAsync(ep =>
                ep.IdElemento == elementoProceso.IdElemento &&
                ep.IdProceso == elementoProceso.IdProceso))
            {
                return Conflict(new { Message = "Este elemento ya está asociado a este proceso." });
            }

            elementoProceso.Validado = false;
            elementoProceso.QuedoEnSena = false;

            _context.ElementoProceso.Add(elementoProceso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetElementoProceso),
                new { id = elementoProceso.IdElementoProceso },
                elementoProceso);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteElementoProceso(int id)
        {
            var elementoProceso = await _context.ElementoProceso.FindAsync(id);
            if (elementoProceso == null)
                return NotFound(new { Message = "ElementoProceso no encontrado." });

            _context.ElementoProceso.Remove(elementoProceso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DTO Classes
        public class AutoRegisterRequest
        {
            public int IdProceso { get; set; }
            public string TipoPropietario { get; set; }
            public int IdPropietario { get; set; }
        }

        public class AgregarPendientesRequest
        {
            public int IdPropietario { get; set; }
            public string TipoPropietario { get; set; }
            public int IdProcesoSalida { get; set; }
        }

        public class ElementoProcesoUpdateDto
        {
            public bool? QuedoEnSena { get; set; }
            public bool? Validado { get; set; }
        }
    }
}