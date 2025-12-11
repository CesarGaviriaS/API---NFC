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
    public class ProgramasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProgramasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Programas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Programa>>> GetPrograma()
        {
            return await _context.Programa.ToListAsync();
        }

        // GET: api/Programas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Programa>> GetPrograma(int id)
        {
            var programa = await _context.Programa.FindAsync(id);

            if (programa == null)
            {
                return NotFound();
            }

            return programa;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrograma(int id, [FromBody] Programa programa)
        {
            // ⚠️ Elimina validación de la propiedad de navegación "Fichas"
            ModelState.Remove(nameof(Programa.Fichas));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Programa.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Actualizar campos
            existing.NombrePrograma = programa.NombrePrograma;
            existing.Codigo = programa.Codigo;
            existing.NivelFormacion = programa.NivelFormacion;
            existing.Estado = programa.Estado;
            existing.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Programas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Programa>> PostPrograma(Programa programa)
        {
            _context.Programa.Add(programa);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPrograma", new { id = programa.IdPrograma }, programa);
        }

        // DELETE: api/Programas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrograma(int id)
        {
            var programa = await _context.Programa.FindAsync(id);
            if (programa == null)
            {
                return NotFound();
            }

            _context.Programa.Remove(programa);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProgramaExists(int id)
        {
            return _context.Programa.Any(e => e.IdPrograma == id);
        }
        // GET: api/Programas/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult> GetProgramasPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            const int maxPageSize = 100;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var totalCount = await _context.Programa.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await _context.Programa
                .AsNoTracking()
                .OrderBy(p => p.IdPrograma)
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
