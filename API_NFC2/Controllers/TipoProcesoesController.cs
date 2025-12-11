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
    public class TipoProcesoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TipoProcesoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/TipoProcesoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoProceso>>> GetTipoProcesos()
        {
            return await _context.TipoProceso.AsNoTracking().ToListAsync();
        }

        // ✅ GET: api/TipoProcesoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoProceso>> GetTipoProceso(int id)
        {
            var tipoProceso = await _context.TipoProceso.FindAsync(id);
            if (tipoProceso == null)
                return NotFound();

            return tipoProceso;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoProceso(int id, [FromBody] TipoProceso tipoProceso)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.TipoProceso.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Actualizar campos
            existing.Tipo = tipoProceso.Tipo;
            existing.Estado = tipoProceso.Estado;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ POST: api/TipoProcesoes
        [HttpPost]
        public async Task<ActionResult<TipoProceso>> PostTipoProceso([FromBody] TipoProceso tipoProceso)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.TipoProceso.Add(tipoProceso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTipoProceso), new { id = tipoProceso.IdTipoProceso }, tipoProceso);
        }

        // ✅ DELETE: api/TipoProcesoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoProceso(int id)
        {
            var tipoProceso = await _context.TipoProceso.FindAsync(id);
            if (tipoProceso == null)
                return NotFound();

            _context.TipoProceso.Remove(tipoProceso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Paginación
        [HttpGet("paged")]
        public async Task<ActionResult> GetTipoProcesosPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            const int maxPageSize = 100;
            pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;

            var totalCount = await _context.TipoProceso.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await _context.TipoProceso
                .OrderBy(t => t.IdTipoProceso)
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

            return Ok(new { Items = items, metadata });
        }
    }
}
