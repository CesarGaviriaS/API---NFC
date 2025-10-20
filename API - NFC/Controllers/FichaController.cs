using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Academico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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

        // GET: api/ficha
        // Obtiene todas las fichas activas, incluyendo la información del programa asociado.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ficha>>> GetFichas()
        {
            return await _context.Fichas
                .Include(f => f.Programa) // <-- Magia aquí: carga el objeto Programa relacionado.
                .Where(f => f.Estado == true)
                .ToListAsync();
        }

        // GET: api/ficha/5
        // Obtiene una ficha específica por ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<Ficha>> GetFicha(int id)
        {
            var ficha = await _context.Fichas
                .Include(f => f.Programa)
                .FirstOrDefaultAsync(f => f.IdFicha == id);

            if (ficha == null || !ficha.Estado)
            {
                return NotFound();
            }

            return ficha;
        }

        // POST: api/ficha
        // Crea una nueva ficha.
        [HttpPost]
        public async Task<ActionResult<Ficha>> PostFicha(Ficha ficha)
        {
            // Validamos que el programa al que se asocia exista y esté activo.
            if (ficha.IdPrograma.HasValue)
            {
                var programaExiste = await _context.Programas.AnyAsync(p => p.IdPrograma == ficha.IdPrograma && p.Estado);
                if (!programaExiste)
                {
                    return BadRequest("El Programa especificado no existe o está inactivo.");
                }
            }

            ficha.Estado = true;
            _context.Fichas.Add(ficha);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFicha), new { id = ficha.IdFicha }, ficha);
        }

        // PUT: api/ficha/5
        // Actualiza una ficha existente.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFicha(int id, Ficha ficha)
        {
            if (id != ficha.IdFicha)
            {
                return BadRequest("El ID de la URL no coincide con el ID de la ficha.");
            }

            // Validamos que el programa al que se asocia exista y esté activo (si se está cambiando).
            if (ficha.IdPrograma.HasValue)
            {
                var programaExiste = await _context.Programas.AnyAsync(p => p.IdPrograma == ficha.IdPrograma && p.Estado);
                if (!programaExiste)
                {
                    return BadRequest("El Programa especificado no existe o está inactivo.");
                }
            }

            _context.Entry(ficha).State = EntityState.Modified;
            // Nos aseguramos de no modificar el estado en una actualización normal.
            _context.Entry(ficha).Property(x => x.Estado).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Fichas.Any(e => e.IdFicha == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ficha/5
        // Desactiva una ficha (borrado lógico).
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFicha(int id)
        {
            var ficha = await _context.Fichas.FindAsync(id);
            if (ficha == null)
            {
                return NotFound();
            }

            ficha.Estado = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("paginated")]
        public async Task<ActionResult<object>> GetFichaPaginated(
            [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = ""
        )
        {
            var query = _context.Fichas
                .Include(f => f.Programa)
                .Where(f => f.Estado == true);
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f =>
                (!string.IsNullOrEmpty(f.Codigo) && f.Codigo.Contains(search)) ||
                (f.Programa != null && !string.IsNullOrEmpty(f.Programa.NombrePrograma) && f.Programa.NombrePrograma.Contains(search)));
            }

            var totalRecords = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;

            if (page > totalPages && totalPages > 0) page = totalPages;
            
            var fichas = await query.
                Skip((page - 1) * pageSize).
                Take(pageSize).
                ToListAsync();

            return new
            {
                Data = fichas,
                page = page,
                pageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

    }
}