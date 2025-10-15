using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Inventario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElementoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ElementoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/elemento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Elemento>>> GetElementos()
        {
            return await _context.Elementos
                .Include(e => e.TipoElemento)
                .Include(e => e.Propietario)
                    .ThenInclude(u => u.Aprendiz)
                .Include(e => e.Propietario)
                    .ThenInclude(u => u.Funcionario)
                .Where(e => e.Estado == true)
                .ToListAsync();
        }

        // GET: api/elemento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Elemento>> GetElemento(int id)
        {
            var elemento = await _context.Elementos
                .Include(e => e.TipoElemento)
                .Include(e => e.Propietario)
                    .ThenInclude(u => u.Aprendiz)
                .Include(e => e.Propietario)
                    .ThenInclude(u => u.Funcionario)
                .FirstOrDefaultAsync(e => e.IdElemento == id);

            if (elemento == null || !elemento.Estado)
            {
                return NotFound();
            }

            return elemento;
        }

        // POST: api/elemento
        // Crea un nuevo elemento.
        [HttpPost]
        public async Task<ActionResult<Elemento>> PostElemento(Elemento elemento)
        {
            // --- Validación de Claves Foráneas ---
            if (elemento.IdTipoElemento.HasValue)
            {
                var tipoElementoExiste = await _context.TiposElemento.AnyAsync(t => t.IdTipoElemento == elemento.IdTipoElemento && t.Estado);
                if (!tipoElementoExiste)
                {
                    return BadRequest("El Tipo de Elemento especificado no existe o está inactivo.");
                }
            }

            if (elemento.IdPropietario.HasValue)
            {
                var propietarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == elemento.IdPropietario && u.Estado);
                if (!propietarioExiste)
                {
                    return BadRequest("El Propietario (Usuario) especificado no existe o está inactivo.");
                }
            }

            elemento.Estado = true;
            _context.Elementos.Add(elemento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetElemento), new { id = elemento.IdElemento }, elemento);
        }

        // PUT: api/elemento/5
        // Actualiza un elemento existente.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutElemento(int id, Elemento elemento)
        {
            if (id != elemento.IdElemento)
            {
                return BadRequest();
            }

            // --- Validación de Claves Foráneas (similar a POST) ---
            if (elemento.IdTipoElemento.HasValue)
            {
                var tipoElementoExiste = await _context.TiposElemento.AnyAsync(t => t.IdTipoElemento == elemento.IdTipoElemento && t.Estado);
                if (!tipoElementoExiste)
                {
                    return BadRequest("El Tipo de Elemento especificado no existe o está inactivo.");
                }
            }

            if (elemento.IdPropietario.HasValue)
            {
                var propietarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == elemento.IdPropietario && u.Estado);
                if (!propietarioExiste)
                {
                    return BadRequest("El Propietario (Usuario) especificado no existe o está inactivo.");
                }
            }

            _context.Entry(elemento).State = EntityState.Modified;
            _context.Entry(elemento).Property(x => x.Estado).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Elementos.Any(e => e.IdElemento == id))
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

        // DELETE: api/elemento/5
        // Desactiva un elemento (borrado lógico).
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteElemento(int id)
        {
            var elemento = await _context.Elementos.FindAsync(id);
            if (elemento == null)
            {
                return NotFound();
            }

            elemento.Estado = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}