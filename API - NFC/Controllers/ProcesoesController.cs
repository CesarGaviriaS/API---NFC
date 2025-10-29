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
    public class ProcesoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProcesoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Procesoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proceso>>> GetProcesos()
        {
            var procesos = await _context.Proceso
                .Include(p => p.TipoProceso)
                .Include(p => p.Usuario)
                .Include(p => p.Aprendiz)
                .AsNoTracking()
                .ToListAsync();

            return Ok(procesos);
        }

        // ✅ GET: api/Procesoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Proceso>> GetProceso(int id)
        {
            var proceso = await _context.Proceso
                .Include(p => p.TipoProceso)
                .Include(p => p.Usuario)
                .Include(p => p.Aprendiz)
                .FirstOrDefaultAsync(p => p.IdProceso == id);

            if (proceso == null)
                return NotFound(new { message = "Proceso no encontrado." });

            return Ok(proceso);
        }

        // ✅ PUT: api/Procesoes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProceso(int id, [FromBody] Proceso proceso)
        {
            // Limpia relaciones de navegación
            ModelState.Remove(nameof(Proceso.Aprendiz));
            ModelState.Remove(nameof(Proceso.Usuario));
            ModelState.Remove(nameof(Proceso.TipoProceso));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Proceso.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Proceso no encontrado." });

            // Validación del tipo de persona
            if (proceso.TipoPersona != "Usuario" && proceso.TipoPersona != "Aprendiz")
                return BadRequest(new { message = "TipoPersona debe ser 'Usuario' o 'Aprendiz'." });

            try
            {
                // Actualiza campos
                existing.IdAprendiz = proceso.IdAprendiz;
                existing.IdUsuario = proceso.IdUsuario;
                existing.IdTipoProceso = proceso.IdTipoProceso;
                existing.TipoPersona = proceso.TipoPersona;
                existing.RequiereOtrosProcesos = proceso.RequiereOtrosProcesos;
                existing.Observaciones = proceso.Observaciones;
                existing.SincronizadoBD = proceso.SincronizadoBD;
                existing.TimeStampEntradaSalida = proceso.TimeStampEntradaSalida ?? DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Proceso actualizado correctamente.",
                    proceso = existing
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al actualizar el proceso.",
                    detalle = ex.Message
                });
            }
        }

        // ✅ POST: api/Procesoes
        [HttpPost]
        public async Task<ActionResult<Proceso>> PostProceso([FromBody] Proceso proceso)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar tipo persona
            if (proceso.TipoPersona != "Usuario" && proceso.TipoPersona != "Aprendiz")
                return BadRequest(new { message = "TipoPersona debe ser 'Usuario' o 'Aprendiz'." });

            proceso.TimeStampEntradaSalida ??= DateTime.Now;
            proceso.SincronizadoBD ??= false;
            proceso.RequiereOtrosProcesos ??= false;

            try
            {
                _context.Proceso.Add(proceso);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProceso), new { id = proceso.IdProceso }, proceso);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear el proceso.",
                    detalle = ex.Message
                });
            }
        }

        // ✅ DELETE: api/Procesoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProceso(int id)
        {
            var proceso = await _context.Proceso.FindAsync(id);
            if (proceso == null)
                return NotFound(new { message = "Proceso no encontrado." });

            try
            {
                _context.Proceso.Remove(proceso);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Proceso eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al eliminar el proceso.",
                    detalle = ex.Message
                });
            }
        }

        // ✅ GET: api/Procesoes/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult> GetProcesosPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            const int maxPageSize = 100;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var totalCount = await _context.Proceso.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await _context.Proceso
                .Include(p => p.TipoProceso)
                .Include(p => p.Usuario)
                .Include(p => p.Aprendiz)
                .AsNoTracking()
                .OrderByDescending(p => p.TimeStampEntradaSalida)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var metadata = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            Response.Headers["X-Pagination"] = System.Text.Json.JsonSerializer.Serialize(metadata);

            return Ok(new
            {
                Items = items,
                metadata.PageNumber,
                metadata.PageSize,
                metadata.TotalCount,
                metadata.TotalPages
            });
        }

        private bool ProcesoExists(int id)
        {
            return _context.Proceso.Any(e => e.IdProceso == id);
        }
    }
}
