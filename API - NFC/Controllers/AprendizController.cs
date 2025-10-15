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
        // Crea un nuevo Aprendiz y su Usuario correspondiente en una transacción.
        [HttpPost]
        public async Task<ActionResult<Aprendiz>> PostAprendiz(Aprendiz aprendiz)
        {
            // --- Lógica de Creación en Dos Pasos ---
            // Usamos una transacción para asegurar que ambas operaciones (crear Aprendiz y crear Usuario)
            // se completen con éxito, o ninguna lo haga.
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Validar y crear el Aprendiz
                    if (aprendiz.IdFicha.HasValue)
                    {
                        var fichaExiste = await _context.Fichas.AnyAsync(f => f.IdFicha == aprendiz.IdFicha && f.Estado);
                        if (!fichaExiste) return BadRequest("La Ficha especificada no existe o está inactiva.");
                    }

                    aprendiz.Estado = true;
                    _context.Aprendices.Add(aprendiz);
                    await _context.SaveChangesAsync(); // Guardamos para obtener el ID del nuevo aprendiz

                    // 2. Crear el Usuario que lo "envuelve"
                    var nuevoUsuario = new Usuario
                    {
                        IdAprendiz = aprendiz.IdAprendiz, // Lo asociamos con el ID que acabamos de crear
                        IdFuncionario = null,
                        Estado = true
                    };
                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();

                    // Si todo sale bien, confirmamos la transacción
                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(GetAprendiz), new { id = aprendiz.IdAprendiz }, aprendiz);
                }
                catch (Exception)
                {
                    // Si algo falla, revertimos todos los cambios
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Ocurrió un error interno al crear el aprendiz y el usuario asociado.");
                }
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

            _context.Entry(aprendiz).State = EntityState.Modified;
            _context.Entry(aprendiz).Property(x => x.Estado).IsModified = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/aprendiz/5
        // Desactiva un aprendiz Y su usuario asociado.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAprendiz(int id)
        {
            var aprendiz = await _context.Aprendices.FindAsync(id);
            if (aprendiz == null)
            {
                return NotFound();
            }

            // Buscamos el usuario asociado para también desactivarlo
            var usuarioAsociado = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdAprendiz == id);

            aprendiz.Estado = false;
            if (usuarioAsociado != null)
            {
                usuarioAsociado.Estado = false;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}