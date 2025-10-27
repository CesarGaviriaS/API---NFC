using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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

        // GET: api/aprendiz
        // Obtiene todos los aprendices activos, incluyendo su Ficha y el Programa de la Ficha.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aprendiz>>> GetAprendices()
        {
            return await _context.Aprendices
                .Include(a => a.Ficha)
                    .ThenInclude(f => f.Programa) // Encadenamos para traer el programa dentro de la ficha
                .Where(a => a.Estado == true)
                .ToListAsync();
        }

        // GET: api/aprendiz/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Aprendiz>> GetAprendiz(int id)
        {
            var aprendiz = await _context.Aprendices
                .Include(a => a.Ficha)
                    .ThenInclude(f => f.Programa)
                .FirstOrDefaultAsync(a => a.IdAprendiz == id);

            if (aprendiz == null || !aprendiz.Estado)
            {
                return NotFound();
            }

            return aprendiz;
        }

        // POST: api/aprendiz
        // Crea un nuevo Aprendiz.
        [HttpPost]
        public async Task<ActionResult<Aprendiz>> PostAprendiz(Aprendiz aprendiz)
        {
            try
            {
                // Validar que la Ficha existe
                var fichaExiste = await _context.Fichas.AnyAsync(f => f.IdFicha == aprendiz.IdFicha && f.Estado);
                if (!fichaExiste)
                {
                    return BadRequest("La Ficha especificada no existe o está inactiva.");
                }

                aprendiz.Estado = true;
                aprendiz.FechaCreacion = DateTime.Now;
                aprendiz.FechaActualizacion = DateTime.Now;
                
                _context.Aprendices.Add(aprendiz);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAprendiz), new { id = aprendiz.IdAprendiz }, aprendiz);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error interno al crear el aprendiz.");
            }
        }

        // PUT: api/aprendiz/5
        // Actualiza un aprendiz existente.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAprendiz(int id, Aprendiz aprendiz)
        {
            if (id != aprendiz.IdAprendiz)
            {
                return BadRequest();
            }

            aprendiz.FechaActualizacion = DateTime.Now;
            
            _context.Entry(aprendiz).State = EntityState.Modified;
            _context.Entry(aprendiz).Property(x => x.Estado).IsModified = false;
            _context.Entry(aprendiz).Property(x => x.FechaCreacion).IsModified = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/aprendiz/5
        // Desactiva un aprendiz.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAprendiz(int id)
        {
            var aprendiz = await _context.Aprendices.FindAsync(id);
            if (aprendiz == null)
            {
                return NotFound();
            }

            aprendiz.Estado = false;
            aprendiz.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}