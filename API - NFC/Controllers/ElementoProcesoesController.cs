using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        // ✅ GET: api/ElementoProcesoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ElementoProceso>>> GetElementoProcesos()
        {
            return await _context.ElementoProceso
                .Include(e => e.Elemento)
                .Include(e => e.Proceso)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ GET: api/ElementoProcesoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ElementoProceso>> GetElementoProceso(int id)
        {
            var elementoProceso = await _context.ElementoProceso
                .Include(e => e.Elemento)
                .Include(e => e.Proceso)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdElementoProceso == id);

            if (elementoProceso == null)
                return NotFound();

            return elementoProceso;
        }

        // ✅ GET: api/ElementoProcesoes/byProceso/5
        [HttpGet("byProceso/{idProceso}")]
        public async Task<ActionResult<IEnumerable<ElementoProceso>>> GetByProceso(int idProceso)
        {
            var relaciones = await _context.ElementoProceso
                .Where(e => e.IdProceso == idProceso)
                .Include(e => e.Elemento)
                .ToListAsync();

            if (!relaciones.Any())
                return NotFound(new { Message = "No se encontraron elementos asociados a este proceso." });

            return relaciones;
        }

        // ✅ PUT: api/ElementoProcesoes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutElementoProceso(int id, [FromBody] ElementoProceso elementoProceso)
        {
            if (id != elementoProceso.IdElementoProceso)
                return BadRequest("El ID del elemento proceso no coincide.");

            var existing = await _context.ElementoProceso.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.IdElemento = elementoProceso.IdElemento;
            existing.IdProceso = elementoProceso.IdProceso;
            existing.Validado = elementoProceso.Validado;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ POST: api/ElementoProcesoes
        [HttpPost]
        public async Task<ActionResult<ElementoProceso>> PostElementoProceso([FromBody] ElementoProceso elementoProceso)
        {
            // Validar existencia de FKs
            if (!await _context.Elemento.AnyAsync(e => e.IdElemento == elementoProceso.IdElemento))
                return BadRequest(new { Message = "El elemento asociado no existe." });

            if (!await _context.Proceso.AnyAsync(p => p.IdProceso == elementoProceso.IdProceso))
                return BadRequest(new { Message = "El proceso asociado no existe." });

            elementoProceso.Validado ??= false; // default false

            _context.ElementoProceso.Add(elementoProceso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetElementoProceso), new { id = elementoProceso.IdElementoProceso }, elementoProceso);
        }

        // ✅ DELETE: api/ElementoProcesoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteElementoProceso(int id)
        {
            var elementoProceso = await _context.ElementoProceso.FindAsync(id);
            if (elementoProceso == null)
                return NotFound();

            _context.ElementoProceso.Remove(elementoProceso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ElementoProcesoExists(int id)
        {
            return _context.ElementoProceso.Any(e => e.IdElementoProceso == id);
        }
    }
}
