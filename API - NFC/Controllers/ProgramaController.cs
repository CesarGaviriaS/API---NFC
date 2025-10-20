using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Academico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProgramaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/programa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Programa>>> GetProgramas()
        {
            return await _context.Programas.Where(p => p.Estado == true).ToListAsync();
        }

        // GET: api/programa/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Programa>> GetPrograma(int id)
        {
            var programa = await _context.Programas.FindAsync(id);
            if (programa == null || !programa.Estado)
            {
                return NotFound();
            }
            return programa;
        }

        // POST: api/programa
        [HttpPost]
        public async Task<ActionResult<Programa>> PostPrograma(Programa programa)
        {
            programa.Estado = true;
            _context.Programas.Add(programa);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPrograma), new { id = programa.IdPrograma }, programa);
        }

        // PUT: api/programa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrograma(int id, Programa programa)
        {
            if (id != programa.IdPrograma)
            {
                return BadRequest();
            }
            _context.Entry(programa).State = EntityState.Modified;
            _context.Entry(programa).Property(x => x.Estado).IsModified = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/programa/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrograma(int id)
        {
            var programa = await _context.Programas.FindAsync(id);
            if (programa == null)
            {
                return NotFound();
            }
            programa.Estado = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpGet("paginated")]
        public async Task<ActionResult<object>> GetProgramasPaginated(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null)
        {
            var query = _context.Programas.Where(t => t.Estado == true);

            // Búsqueda 
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

            // Validaciones de página
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var programas = await query
                .OrderBy(t => t.IdPrograma)
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
