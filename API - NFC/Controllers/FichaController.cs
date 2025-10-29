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
            existing.IdPrograma = ficha.IdPrograma;
            existing.Codigo = ficha.Codigo;
            existing.FechaInicio = ficha.FechaInicio;
            existing.FechaFinal = ficha.FechaFinal;
            existing.Estado = ficha.Estado;
            existing.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // POST: api/Ficha
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Ficha>> PostFicha(Ficha ficha)
        {
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
