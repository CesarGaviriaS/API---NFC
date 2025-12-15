using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_NFC.Data;
using API___NFC.Models;
using BCryptNet = BCrypt.Net.BCrypt;

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

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Usuario.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Validar tipo documento
            var tiposValidos = new[] { "CC", "TI", "CE", "PA" };
            if (!tiposValidos.Contains(usuario.TipoDocumento))
                return BadRequest("El tipo de documento debe ser CC, TI, CE o PA");

            // Validar rol
            var rolesValidos = new[] { "Administrador", "Guardia", "Funcionario" };
            if (!rolesValidos.Contains(usuario.Rol))
                return BadRequest("El rol debe ser Administrador, Guardia o Funcionario");

            // ⚠️ VALIDACIÓN DE DUPLICADOS (Código de barras ahora es opcional)
            bool existeDuplicado = await _context.Usuario.AnyAsync(u =>
                u.IdUsuario != id &&
                (
                    u.NumeroDocumento == usuario.NumeroDocumento ||
                    u.Correo == usuario.Correo ||
                    (!string.IsNullOrWhiteSpace(usuario.CodigoBarras) && u.CodigoBarras == usuario.CodigoBarras)
                )
            );

            if (existeDuplicado)
                return Conflict("Ya existe un usuario con ese documento, correo o código de barras.");

            // Normalizar Código de Barras vacío → null
            existing.CodigoBarras = string.IsNullOrWhiteSpace(usuario.CodigoBarras)
                ? null
                : usuario.CodigoBarras;

            // Actualizar campos
            existing.Nombre = usuario.Nombre;
            existing.Apellido = usuario.Apellido;
            existing.TipoDocumento = usuario.TipoDocumento;
            existing.NumeroDocumento = usuario.NumeroDocumento;
            existing.Correo = usuario.Correo;
            existing.Rol = usuario.Rol;
            existing.Cargo = usuario.Cargo;
            existing.Telefono = usuario.Telefono;
            existing.FotoUrl = usuario.FotoUrl;
            existing.Estado = usuario.Estado ?? true;
            existing.FechaActualizacion = DateTime.UtcNow;

            // Ensure other dates are UTC if they exist (PostgreSQL fix)
            if (existing.FechaCreacion.HasValue && existing.FechaCreacion.Value.Kind == DateTimeKind.Local)
                existing.FechaCreacion = existing.FechaCreacion.Value.ToUniversalTime();
            
            if (existing.FechaTokenExpira.HasValue && existing.FechaTokenExpira.Value.Kind == DateTimeKind.Local)
                existing.FechaTokenExpira = existing.FechaTokenExpira.Value.ToUniversalTime();

            // Actualizar contraseña solo si viene nueva
            if (!string.IsNullOrWhiteSpace(usuario.Contraseña))
                existing.Contraseña = HashPasswordIfNeeded(usuario.Contraseña);

            await _context.SaveChangesAsync();
            return NoContent();
        }


        // ✅ POST: api/Usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(usuario.Contraseña))
            {
                return BadRequest("La contraseña es requerida y no puede estar vacía.");
            }

            // 🔹 Validar duplicados
            if (await _context.Usuario.AnyAsync(u =>
                    u.NumeroDocumento == usuario.NumeroDocumento ||
                    u.Correo == usuario.Correo))
            {
                return Conflict("Ya existe un usuario con ese número de documento o correo electrónico.");
            }

            usuario.Estado ??= true;
            usuario.FechaCreacion = DateTime.UtcNow;
            usuario.FechaActualizacion = DateTime.UtcNow;

            // 🔐 Hashear contraseña antes de guardar
            usuario.Contraseña = HashPasswordIfNeeded(usuario.Contraseña);

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
            usuario.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("byDocumento/{documento}")]
        public async Task<ActionResult<Usuario>> GetUsuarioByDocumento(string documento)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.NumeroDocumento == documento);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
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
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsuario([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<object>());

            query = query.Trim().ToLower();

            var result = await _context.Usuario
                .Where(u => u.Estado == true &&
                       (u.NumeroDocumento.Contains(query) ||
                        u.Nombre.ToLower().Contains(query) ||
                        u.Apellido.ToLower().Contains(query)))
                .Select(u => new {
                    id = u.IdUsuario,
                    nombre = u.Nombre + " " + u.Apellido,
                    documento = u.NumeroDocumento,
                    rol = u.Rol 
                })
                .Take(15)
                .ToListAsync();

            return Ok(result);
        }


        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.IdUsuario == id);
        }

        private static string HashPasswordIfNeeded(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));
            }

            // Evitar doble hash si ya viene en formato BCrypt
            return IsBcryptHash(password)
                ? password
                : BCryptNet.HashPassword(password);
        }

        private static bool IsBcryptHash(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   (value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$"));
        }
    }
}
