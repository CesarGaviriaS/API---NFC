using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Where(u => u.Estado == true)
                .ToListAsync();
        }

        // GET: api/usuario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null || !usuario.Estado)
            {
                return NotFound();
            }
            return usuario;
        }

        // POST: api/usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            try
            {
                usuario.Estado = true;
                usuario.FechaCreacion = DateTime.Now;
                usuario.FechaActualizacion = DateTime.Now;
                
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error interno al crear el usuario.");
            }
        }

        // PUT: api/usuario/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return BadRequest();
            }

            usuario.FechaActualizacion = DateTime.Now;
            
            _context.Entry(usuario).State = EntityState.Modified;
            _context.Entry(usuario).Property(x => x.Estado).IsModified = false;
            _context.Entry(usuario).Property(x => x.FechaCreacion).IsModified = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/usuario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Estado = false;
            usuario.FechaActualizacion = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //GET: api/usuario/paginated
        [HttpGet("paginated")]
        public async Task<ActionResult<object>> GetUsuariosPaginated
            ([FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "" )
        {
            var query = _context.Usuarios
                .Where(u => u.Estado == true);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => 
                    u.Nombre.Contains(search) ||
                    u.Apellido.Contains(search) ||
                    u.NumeroDocumento.Contains(search) ||
                    u.Correo.Contains(search));
            }
            
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords/ (double)pageSize);

            // validamos página
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var usuarios = await query
                .Skip((page -1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new
            {
                Data = usuarios,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }
    }
}