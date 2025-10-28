using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiNfc.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using API___NFC.Models;

namespace ApiNfc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgramasController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public ProgramasController(NfcDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Programa>>> GetAll()
        {
            return await _context.Programas.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Programa>> GetPrograma(int id)
        {
            var item = await _context.Programas.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Programa>> Create(Programa programa)
        {
            _context.Programas.Add(programa);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPrograma), new { id = programa.IdPrograma }, programa);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Programa programa)
        {
            if (id != programa.IdPrograma) return BadRequest();
            _context.Entry(programa).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Programas.AnyAsync(p => p.IdPrograma == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Programas.FindAsync(id);
            if (item == null) return NotFound();
            _context.Programas.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        // Método paginated (extraído del archivo)
        [HttpGet("paginated")]
        public async Task<ActionResult<object>> GetProgramasPaginated(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var query = _context.Programas.Where(p => p.Estado == true);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.NombrePrograma.Contains(search) ||
                    p.Codigo.Contains(search) ||
                    p.IdPrograma.ToString().Contains(search)
                );
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var programas = await query
                .OrderBy(p => p.IdPrograma)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new
            {
                Data = programas,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }
    }
}