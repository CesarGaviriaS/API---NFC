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
    public class AprendizController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AprendizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Aprendiz
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aprendiz>>> GetAprendiz()
        {
            return await _context.Aprendiz.ToListAsync();
        }

        // GET: api/Aprendiz/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Aprendiz>> GetAprendiz(int id)
        {
            var aprendiz = await _context.Aprendiz.FindAsync(id);

            if (aprendiz == null)
            {
                return NotFound();
            }

            return aprendiz;
        }

        // PUT: api/Aprendiz/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAprendiz(int id, Aprendiz aprendiz)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Cargar la entidad existente
            var existing = await _context.Aprendiz.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Mapear sólo los campos permitidos (evita overposting)
            existing.Nombre = aprendiz.Nombre;
            existing.Apellido = aprendiz.Apellido;
            existing.TipoDocumento = aprendiz.TipoDocumento;
            existing.NumeroDocumento = aprendiz.NumeroDocumento;
            existing.Correo = aprendiz.Correo;
            existing.CodigoBarras = aprendiz.CodigoBarras;
            existing.IdFicha = aprendiz.IdFicha;
            existing.Telefono = aprendiz.Telefono;
            existing.FotoUrl = aprendiz.FotoUrl;
            existing.Estado = aprendiz.Estado;
            existing.FechaActualizacion = DateTime.Now; // actualizar la fecha de modificación localmente

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AprendizExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // POST: api/Aprendiz
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Aprendiz>> PostAprendiz(Aprendiz aprendiz)
        {
            _context.Aprendiz.Add(aprendiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAprendiz", new { id = aprendiz.IdAprendiz }, aprendiz);
        }

        // DELETE: api/Aprendiz/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAprendiz(int id)
        {
            var aprendiz = await _context.Aprendiz.FindAsync(id);
            if (aprendiz == null)
            {
                return NotFound();
            }

            _context.Aprendiz.Remove(aprendiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AprendizExists(int id)
        {
            return _context.Aprendiz.Any(e => e.IdAprendiz == id);
        }
        // GET: api/Aprendiz/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult> GetAprendizPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            const int maxPageSize = 100;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var totalCount = await _context.Aprendiz.CountAsync();
            var totalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);

            var items = await _context.Aprendiz
                .AsNoTracking()
                .OrderBy(a => a.IdAprendiz)
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
