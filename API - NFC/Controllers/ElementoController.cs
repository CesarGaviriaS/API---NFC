using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Constants;
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
                .Where(e => e.Estado == true)
                .ToListAsync();
        }

        // GET: api/elemento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Elemento>> GetElemento(int id)
        {
            var elemento = await _context.Elementos
                .Include(e => e.TipoElemento)
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
            var tipoElementoExiste = await _context.TiposElemento.AnyAsync(t => t.IdTipoElemento == elemento.IdTipoElemento && t.Estado);
            if (!tipoElementoExiste)
            {
                return BadRequest("El Tipo de Elemento especificado no existe o está inactivo.");
            }

            // Validate TipoPropietario and IdPropietario
            if (elemento.TipoPropietario == AppConstants.OwnerTypes.Aprendiz)
            {
                var aprendizExiste = await _context.Aprendices.AnyAsync(a => a.IdAprendiz == elemento.IdPropietario && a.Estado);
                if (!aprendizExiste)
                {
                    return BadRequest("El Aprendiz especificado no existe o está inactivo.");
                }
            }
            else if (elemento.TipoPropietario == AppConstants.OwnerTypes.Usuario)
            {
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == elemento.IdPropietario && u.Estado);
                if (!usuarioExiste)
                {
                    return BadRequest("El Usuario especificado no existe o está inactivo.");
                }
            }
            else
            {
                return BadRequest("TipoPropietario debe ser 'Aprendiz' o 'Usuario'.");
            }

            elemento.Estado = true;
            elemento.FechaCreacion = DateTime.Now;
            elemento.FechaActualizacion = DateTime.Now;
            
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
            var tipoElementoExiste = await _context.TiposElemento.AnyAsync(t => t.IdTipoElemento == elemento.IdTipoElemento && t.Estado);
            if (!tipoElementoExiste)
            {
                return BadRequest("El Tipo de Elemento especificado no existe o está inactivo.");
            }

            // Validate TipoPropietario and IdPropietario
            if (elemento.TipoPropietario == AppConstants.OwnerTypes.Aprendiz)
            {
                var aprendizExiste = await _context.Aprendices.AnyAsync(a => a.IdAprendiz == elemento.IdPropietario && a.Estado);
                if (!aprendizExiste)
                {
                    return BadRequest("El Aprendiz especificado no existe o está inactivo.");
                }
            }
            else if (elemento.TipoPropietario == AppConstants.OwnerTypes.Usuario)
            {
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == elemento.IdPropietario && u.Estado);
                if (!usuarioExiste)
                {
                    return BadRequest("El Usuario especificado no existe o está inactivo.");
                }
            }
            else
            {
                return BadRequest("TipoPropietario debe ser 'Aprendiz' o 'Usuario'.");
            }

            elemento.FechaActualizacion = DateTime.Now;
            
            _context.Entry(elemento).State = EntityState.Modified;
            _context.Entry(elemento).Property(x => x.Estado).IsModified = false;
            _context.Entry(elemento).Property(x => x.FechaCreacion).IsModified = false;

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
            elemento.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/elemento/paginated
        [HttpGet("paginated")]
        public async Task<ActionResult<object>> GetElementosPaginated(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "")
        {
            var query = _context.Elementos
                .Include(e => e.TipoElemento)
                .Where(e => e.Estado == true);

            // Aplicar búsqueda si existe
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    (e.Marca != null && e.Marca.Contains(search)) ||
                    (e.Modelo != null && e.Modelo.Contains(search)) ||
                    (e.Serial != null && e.Serial.Contains(search))
                );
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Validar página
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var elementos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new
            {
                Data = elementos,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }
    }
}