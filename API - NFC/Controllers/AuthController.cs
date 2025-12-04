using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API_NFC.Data;
using API___NFC.Models;
using BCrypt.Net;
using System.Text.RegularExpressions;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contraseña))
                return BadRequest(new { error = "Correo y contraseña requeridos." });

            var correo = request.Correo.Trim().ToLower();
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo.ToLower() == correo && u.Estado == true);

            if (usuario == null)
                return Unauthorized(new { error = "Usuario no encontrado o inactivo." });

            if (usuario.Rol != "Administrador" && usuario.Rol != "Guardia")
                return Unauthorized(new { error = "Sin permisos (rol)." });

            var ver = VerificarContraseñaYPosibleMigracion(request.Contraseña, usuario.Contraseña);
            if (!ver.EsValida)
                return Unauthorized(new { error = "Contraseña incorrecta." });

            if (ver.NecesitaUpgrade)
            {
                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(request.Contraseña);
                usuario.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var token = GenerateJwtToken(usuario);

            return Ok(new
            {
                message = "OK",
                token,
                usuario = new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Rol,
                    usuario.Correo
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Usuario usuario)
        {
            if (usuario == null)
                return BadRequest(new { error = "Datos inválidos." });

            if (string.IsNullOrWhiteSpace(usuario.Correo) || string.IsNullOrWhiteSpace(usuario.Contraseña) ||
                string.IsNullOrWhiteSpace(usuario.Nombre) || string.IsNullOrWhiteSpace(usuario.Apellido) ||
                string.IsNullOrWhiteSpace(usuario.Rol) || string.IsNullOrWhiteSpace(usuario.NumeroDocumento))
                return BadRequest(new { error = "Campos requeridos faltantes." });

            var correo = usuario.Correo.Trim().ToLower();
            if (await _context.Usuario.AnyAsync(u => u.Correo.ToLower() == correo))
                return Conflict(new { error = "Correo ya existe." });

            usuario.Correo = correo;
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
            usuario.Estado = true;

            if (string.IsNullOrWhiteSpace(usuario.CodigoBarras))
            {
                usuario.CodigoBarras = null;
            }
            else
            {
                usuario.CodigoBarras = usuario.CodigoBarras.Trim();
            }

            usuario.FechaCreacion = DateTime.UtcNow;
            usuario.FechaActualizacion = DateTime.UtcNow;

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Registrado" });
        }

        // ✅ NUEVO: Recuperación simple por documento + correo
        [HttpPost("recuperar-password")]
        public async Task<IActionResult> RecuperarPassword([FromBody] RecuperarPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NumeroDocumento) || string.IsNullOrWhiteSpace(request.Correo))
                return BadRequest(new { error = "Número de documento y correo son requeridos." });

            var documento = request.NumeroDocumento.Trim();
            var correo = request.Correo.Trim().ToLower();

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u =>
                    u.NumeroDocumento == documento &&
                    u.Correo.ToLower() == correo &&
                    u.Estado == true);

            if (usuario == null)
                return NotFound(new { error = "No se encontró un usuario con ese documento y correo." });

            return Ok(new
            {
                message = "Usuario verificado",
                idUsuario = usuario.IdUsuario,
                nombre = usuario.Nombre,
                apellido = usuario.Apellido,
                correo = usuario.Correo
            });
        }

        // ✅ NUEVO: Cambiar contraseña directamente
        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request)
        {
            if (request == null || request.IdUsuario <= 0 || string.IsNullOrWhiteSpace(request.NuevaContraseña))
                return BadRequest(new { error = "Datos incompletos." });

            if (request.NuevaContraseña.Length < 6)
                return BadRequest(new { error = "La contraseña debe tener al menos 6 caracteres." });

            var usuario = await _context.Usuario.FindAsync(request.IdUsuario);

            if (usuario == null)
                return NotFound(new { error = "Usuario no encontrado." });

            if (usuario.Estado != true)
                return BadRequest(new { error = "Usuario inactivo." });

            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(request.NuevaContraseña);
            usuario.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "✅ Contraseña actualizada correctamente. Ya puedes iniciar sesión." });
        }

        // ========== MÉTODOS AUXILIARES ==========

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new Claim("idUsuario", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("nombre", usuario.Nombre),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var keyStr = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(keyStr))
                throw new Exception("Falta Jwt:Key en configuración");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private PasswordCheckResult VerificarContraseñaYPosibleMigracion(string input, string stored)
        {
            if (string.IsNullOrEmpty(stored))
                return new PasswordCheckResult(false, false, "vacío");

            if (stored.StartsWith("$2a$") || stored.StartsWith("$2b$") || stored.StartsWith("$2y$"))
                return new PasswordCheckResult(BCrypt.Net.BCrypt.Verify(input, stored), false, "bcrypt");

            bool isBase64 = stored.Length == 44 && Regex.IsMatch(stored, @"^[A-Za-z0-9+/=]+$");
            if (isBase64)
            {
                using var sha = SHA256.Create();
                var b64 = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
                return new PasswordCheckResult(stored == b64, stored == b64, "sha256-b64");
            }

            bool isHex = stored.Length == 64 && Regex.IsMatch(stored, "^[0-9a-fA-F]+$");
            if (isHex)
            {
                using var sha = SHA256.Create();
                var hex = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLower();
                bool match = stored.Equals(hex, StringComparison.OrdinalIgnoreCase);
                return new PasswordCheckResult(match, match, "sha256-hex");
            }

            if (stored == input)
                return new PasswordCheckResult(true, true, "plaintext");

            return new PasswordCheckResult(false, false, "mismatch");
        }

        // ========== RECORDS Y DTOs ==========

        private record PasswordCheckResult(bool EsValida, bool NecesitaUpgrade, string Info);

        public class LoginRequest
        {
            public string Correo { get; set; } = string.Empty;
            public string Contraseña { get; set; } = string.Empty;
        }

        public class RecuperarPasswordRequest
        {
            public string NumeroDocumento { get; set; } = string.Empty;
            public string Correo { get; set; } = string.Empty;
        }

        public class CambiarPasswordRequest
        {
            public int IdUsuario { get; set; }
            public string NuevaContraseña { get; set; } = string.Empty;
        }
    }
}