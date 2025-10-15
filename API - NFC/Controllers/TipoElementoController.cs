using API___NFC.Data; 
using API___NFC.Models; 
using API___NFC.Models.Entity.Inventario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoElementoController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Tu DbContext

        // El constructor inyecta el DbContext para poder hablar con la base de datos
        public TipoElementoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/tipoelemento
        // Lee todos los tipos de elemento que estén activos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoElemento>>> GetTiposElemento()
        {
            // Filtramos para devolver solo los registros con Estado = true
            return await _context.TiposElemento.Where(t => t.Estado == true).ToListAsync();
        }

        // GET: api/tipoelemento/5
        // Lee un solo tipo de elemento por su ID
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoElemento>> GetTipoElemento(int id)
        {
            var tipoElemento = await _context.TiposElemento.FindAsync(id);

            // Si no se encuentra o está inactivo, devolvemos un error 404
            if (tipoElemento == null || !tipoElemento.Estado)
            {
                return NotFound();
            }

            return tipoElemento;
        }

        // POST: api/tipoelemento
        // Crea un nuevo tipo de elemento
        [HttpPost]
        public async Task<ActionResult<TipoElemento>> PostTipoElemento(TipoElemento tipoElemento)
        {
            // Forzamos el estado a 'true' al crear un nuevo registro
            tipoElemento.Estado = true;
            _context.TiposElemento.Add(tipoElemento);
            await _context.SaveChangesAsync();

            // Devolvemos una respuesta 201 Created con el nuevo objeto
            return CreatedAtAction(nameof(GetTipoElemento), new { id = tipoElemento.IdTipoElemento }, tipoElemento);
        }

        // PUT: api/tipoelemento/5
        // Actualiza un tipo de elemento existente
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoElemento(int id, TipoElemento tipoElemento)
        {
            if (id != tipoElemento.IdTipoElemento)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la petición.");
            }

            // Marcamos la entidad como modificada para que EF la actualice
            _context.Entry(tipoElemento).State = EntityState.Modified;

            // Nos aseguramos de no cambiar el estado 'Estado' en una actualización normal
            // El estado solo se cambia con el borrado lógico (DELETE)
            _context.Entry(tipoElemento).Property(x => x.Estado).IsModified = false;

            await _context.SaveChangesAsync();

            return NoContent(); // Respuesta 204 No Content, significa que todo salió bien
        }

        // DELETE: api/tipoelemento/5
        // Desactiva un tipo de elemento (Borrado Lógico)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoElemento(int id)
        {
            var tipoElemento = await _context.TiposElemento.FindAsync(id);
            if (tipoElemento == null)
            {
                return NotFound();
            }

            // En lugar de borrarlo, cambiamos su estado a 'false'
            tipoElemento.Estado = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}