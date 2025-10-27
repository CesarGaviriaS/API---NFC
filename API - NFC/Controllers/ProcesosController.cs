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
                .Include(p => p.TipoProceso)
                .Include(p => p.Aprendiz)
                .Include(p => p.Usuario)
                .ToListAsync();
        }

        // GET: api/proceso/5
        // Obtiene un proceso específico por su ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<Proceso>> GetProceso(int id)
        {
            var proceso = await _context.Procesos
                .Include(p => p.TipoProceso)
                .Include(p => p.Aprendiz)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.IdProceso == id);

            if (proceso == null)
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
            var tipoProcesoExiste = await _context.TiposProceso.AnyAsync(t => t.IdTipoProceso == proceso.IdTipoProceso && t.Estado);
            if (!tipoProcesoExiste)
            {
                return BadRequest("El Tipo de Proceso no existe o está inactivo.");
            }

            // Validate TipoPersona
            if (proceso.TipoPersona == "Aprendiz" && proceso.IdAprendiz.HasValue)
            {
                var aprendizExiste = await _context.Aprendices.AnyAsync(a => a.IdAprendiz == proceso.IdAprendiz && a.Estado);
                if (!aprendizExiste)
                {
                    return BadRequest("El Aprendiz no existe o está inactivo.");
                }
            }
            else if (proceso.TipoPersona == "Usuario" && proceso.IdUsuario.HasValue)
            {
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == proceso.IdUsuario && u.Estado);
                if (!usuarioExiste)
                {
                    return BadRequest("El Usuario no existe o está inactivo.");
                }
            }
            else
            {
                return BadRequest("TipoPersona debe ser 'Aprendiz' o 'Usuario' con el ID correspondiente.");
            }

            // Validate Guardia exists (IdGuardia refers to Usuario with Rol='Guardia')
            var guardiaExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == proceso.IdGuardia && u.Rol == "Guardia" && u.Estado);
            if (!guardiaExiste)
            {
                return BadRequest("El Guardia no existe o no tiene el rol correcto.");
            }

            proceso.TimeStampEntradaSalida = DateTime.Now;
            proceso.SincronizadoBD = false;
            
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

            _context.Entry(proceso).State = EntityState.Modified;

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
        // Elimina un registro de proceso.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProceso(int id)
        {
            var proceso = await _context.Procesos.FindAsync(id);
            if (proceso == null)
            {
                return NotFound();
            }

            _context.Procesos.Remove(proceso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}