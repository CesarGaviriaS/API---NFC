using API___NFC.Data;
using API___NFC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuardiaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GuardiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/guardia
        // Obtiene todos los guardias activos.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Guardia>>> GetGuardias()
        {
            return await _context.Guardias.Where(g => g.Estado == true).ToListAsync();
        }

        // GET: api/guardia/5
        // Obtiene un guardia específico por su ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<Guardia>> GetGuardia(int id)
        {
            var guardia = await _context.Guardias.FindAsync(id);

            if (guardia == null || !guardia.Estado)
            {
                return NotFound();
            }

            return guardia;
        }

        // POST: api/guardia
        // Crea un nuevo guardia.
        [HttpPost]
        public async Task<ActionResult<Guardia>> PostGuardia(Guardia guardia)
        {
            guardia.Estado = true;
            _context.Guardias.Add(guardia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGuardia), new { id = guardia.IdGuardia }, guardia);
        }

        // PUT: api/guardia/5
        // Actualiza un guardia existente.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGuardia(int id, Guardia guardia)
        {
            if (id != guardia.IdGuardia)
            {
                return BadRequest("El ID de la URL no coincide con el ID del guardia.");
            }

            _context.Entry(guardia).State = EntityState.Modified;
            _context.Entry(guardia).Property(x => x.Estado).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Guardias.Any(e => e.IdGuardia == id))
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

        // DELETE: api/guardia/5
        // Desactiva un guardia (borrado lógico).
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuardia(int id)
        {
            var guardia = await _context.Guardias.FindAsync(id);
            if (guardia == null)
            {
                return NotFound();
            }

            guardia.Estado = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}