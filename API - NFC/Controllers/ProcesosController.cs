using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Proceso;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcesoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProcesoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/proceso
        // Obtiene todos los procesos activos, incluyendo todas sus relaciones.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proceso>>> GetProcesos()
        {
            return await _context.Procesos
                .Include(p => p.TipoProceso) // Carga el Tipo de Proceso.
                .Include(p => p.Elemento)    // Carga el Elemento.
                .Include(p => p.Portador)    // Carga el Usuario (Portador).
                .Where(p => p.Estado == true)
                .ToListAsync();
        }

        // GET: api/proceso/5
        // Obtiene un proceso específico por su ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<Proceso>> GetProceso(int id)
        {
            var proceso = await _context.Procesos
                .Include(p => p.TipoProceso)
                .Include(p => p.Elemento)
                .Include(p => p.Portador)
                .FirstOrDefaultAsync(p => p.IdProceso == id);

            if (proceso == null || !proceso.Estado)
            {
                return NotFound();
            }

            return proceso;
        }

        // POST: api/proceso
        // Crea un nuevo registro de proceso.
        [HttpPost]
        public async Task<ActionResult<Proceso>> PostProceso(Proceso proceso)
        {
            // --- Validación de todas las claves foráneas ---
            if (proceso.IdTipoProceso.HasValue)
            {
                var tipoProcesoExiste = await _context.TiposProceso.AnyAsync(t => t.IdTipoProceso == proceso.IdTipoProceso && t.Estado);
                if (!tipoProcesoExiste) return BadRequest("El Tipo de Proceso no existe o está inactivo.");
            }

            if (proceso.IdElemento.HasValue)
            {
                var elementoExiste = await _context.Elementos.AnyAsync(e => e.IdElemento == proceso.IdElemento && e.Estado);
                if (!elementoExiste) return BadRequest("El Elemento no existe o está inactivo.");
            }

            if (proceso.IdPortador.HasValue)
            {
                var portadorExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == proceso.IdPortador && u.Estado);
                if (!portadorExiste) return BadRequest("El Portador (Usuario) no existe o está inactivo.");
            }

            proceso.Estado = true;
            _context.Procesos.Add(proceso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProceso), new { id = proceso.IdProceso }, proceso);
        }

        // PUT: api/proceso/5
        // Actualiza un proceso existente.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProceso(int id, Proceso proceso)
        {
            if (id != proceso.IdProceso)
            {
                return BadRequest();
            }

            // (Opcional) Puedes añadir aquí las mismas validaciones de claves foráneas del POST si quieres permitir que se cambien.

            _context.Entry(proceso).State = EntityState.Modified;
            _context.Entry(proceso).Property(x => x.Estado).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Procesos.Any(e => e.IdProceso == id))
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

        // DELETE: api/proceso/5
        // Desactiva un registro de proceso (borrado lógico).
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProceso(int id)
        {
            var proceso = await _context.Procesos.FindAsync(id);
            if (proceso == null)
            {
                return NotFound();
            }

            proceso.Estado = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}