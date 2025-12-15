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
    public class FichaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FichaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Ficha
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ficha>>> GetFichas()
        {
            return await _context.Ficha.ToListAsync();
        }

        // GET: api/Ficha/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ficha>> GetFicha(int id)
        {
            var ficha = await _context.Ficha.FindAsync(id);

            if (ficha == null)
            {
                return NotFound();
            }

            return ficha;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFicha(int id, [FromBody] Ficha ficha)
        {
            // ⚠️ Elimina validación de la propiedad de navegación "Programa"
            ModelState.Remove(nameof(Ficha.Programa));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Ficha.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Actualizar campos
            // Actualizar campos
            existing.IdPrograma = ficha.IdPrograma;
            existing.Codigo = ficha.Codigo;
            // Enforce UTC for dates
            existing.FechaInicio = DateTime.SpecifyKind(ficha.FechaInicio, DateTimeKind.Utc);
            existing.FechaFinal = DateTime.SpecifyKind(ficha.FechaFinal, DateTimeKind.Utc);
            existing.Estado = ficha.Estado;
            existing.FechaActualizacion = DateTime.UtcNow; // Changed from DateTime.Now to UtcNow

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // POST: api/Ficha
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Ficha>> PostFicha(Ficha ficha)
        {
            // Enforce UTC for all dates
            ficha.FechaInicio = DateTime.SpecifyKind(ficha.FechaInicio, DateTimeKind.Utc);
            ficha.FechaFinal = DateTime.SpecifyKind(ficha.FechaFinal, DateTimeKind.Utc);
            ficha.FechaCreacion = DateTime.UtcNow;
            ficha.FechaActualizacion = DateTime.UtcNow;

            _context.Ficha.Add(ficha);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFicha", new { id = ficha.IdFicha }, ficha);
        }

        // DELETE: api/Ficha/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFicha(int id)
        {
            var ficha = await _context.Ficha.FindAsync(id);
            if (ficha == null)
            {
                return NotFound();
            }

            _context.Ficha.Remove(ficha);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchFicha([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<object>());

            query = query.Trim().ToLower();

            var result = await _context.Ficha
                .Where(f =>
                    f.Codigo.ToLower().Contains(query) ||
                    f.IdFicha.ToString().Contains(query)
                )
                .Select(f => new {
                    id = f.IdFicha,
                    codigo = f.Codigo
                })
                .Take(20)
                .ToListAsync();

            return Ok(result);
        }

        private bool FichaExists(int id)
        {
            return _context.Ficha.Any(e => e.IdFicha == id);
        }
        // GET: api/Ficha/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult> GetFichasPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            const int maxPageSize = 100;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var totalCount = await _context.Ficha.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await _context.Ficha
                .AsNoTracking()
                .OrderBy(f => f.IdFicha)
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
    }
}
