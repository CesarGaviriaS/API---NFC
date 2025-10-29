using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_NFC.Data;
using API___NFC.Models;

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

        // ✅ GET: api/Usuario (solo activos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuario
                .Where(u => u.Estado == true)
                .AsNoTracking()
                .OrderBy(u => u.Nombre)
                .ToListAsync();
        }

        // ✅ GET: api/Usuario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            return Ok(usuario);
        }

        // ✅ PUT: api/Usuario/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Usuario.FindAsync(id);
            if (existing == null)
                return NotFound();

            // 🔹 Validar duplicado de documento o correo
            if (await _context.Usuario.AnyAsync(u =>
                    (u.NumeroDocumento == usuario.NumeroDocumento || u.Correo == usuario.Correo) &&
                    u.IdUsuario != id))
            {
                return Conflict("Ya existe un usuario con ese número de documento o correo electrónico.");
            }

            // 🔹 Actualizar campos
            existing.Nombre = usuario.Nombre;
            existing.Apellido = usuario.Apellido;
            existing.TipoDocumento = usuario.TipoDocumento;
            existing.NumeroDocumento = usuario.NumeroDocumento;
            existing.Correo = usuario.Correo;
            existing.Contraseña = usuario.Contraseña; // ⚠️ En producción, encriptar antes de guardar
            existing.Rol = usuario.Rol;
            existing.CodigoBarras = usuario.CodigoBarras;
            existing.Cargo = usuario.Cargo;
            existing.Telefono = usuario.Telefono;
            existing.FotoUrl = usuario.FotoUrl;
            existing.Estado = usuario.Estado ?? true;
            existing.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ✅ POST: api/Usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 🔹 Validar duplicados
            if (await _context.Usuario.AnyAsync(u =>
                    u.NumeroDocumento == usuario.NumeroDocumento ||
                    u.Correo == usuario.Correo))
            {
                return Conflict("Ya existe un usuario con ese número de documento o correo electrónico.");
            }

            usuario.Estado ??= true;
            usuario.FechaCreacion = DateTime.Now;
            usuario.FechaActualizacion = DateTime.Now;

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
        }

        // ✅ DELETE (Soft Delete): api/Usuario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
                return NotFound();

            usuario.Estado = false;
            usuario.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ GET Paginado
        [HttpGet("paged")]
        public async Task<ActionResult> GetUsuariosPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            const int maxPageSize = 100;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var query = _context.Usuario
                .Where(u => u.Estado == true)
                .AsNoTracking()
                .OrderBy(u => u.Nombre);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var metadata = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            Response.Headers["X-Pagination"] = System.Text.Json.JsonSerializer.Serialize(metadata);

            return Ok(new
            {
                Items = items,
                metadata.PageNumber,
                metadata.PageSize,
                metadata.TotalCount,
                metadata.TotalPages
            });
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.IdUsuario == id);
        }
    }
}
