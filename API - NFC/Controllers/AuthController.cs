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

        // ✅ LOGIN (solo Administradores o Guardias)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.Contraseña))
                return BadRequest("Debe ingresar correo y contraseña.");

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == request.Correo && u.Estado == true);
            if (usuario == null)
                return Unauthorized("Usuario no encontrado o inactivo.");

            if (usuario.Rol != "Administrador" && usuario.Rol != "Guardia")
                return Unauthorized("El usuario no tiene permisos para iniciar sesión.");

            // 🔹 Validar contraseña (BCrypt o SHA256)
            if (!VerificarContraseña(request.Contraseña, usuario.Contraseña))
                return Unauthorized("Contraseña incorrecta.");

            // 🔹 Generar token JWT
            var token = GenerateJwtToken(usuario);

            return Ok(new
            {
                Message = "Inicio de sesión exitoso",
                Token = token,
                Usuario = new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Rol,
                    usuario.Correo
                }
            });
        }

        // ✅ REGISTRO
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Usuario.AnyAsync(u => u.Correo == usuario.Correo))
                return Conflict("Ya existe un usuario con ese correo.");

            // 🔐 Hashear la contraseña con BCrypt
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
            usuario.Estado = true;
            usuario.FechaCreacion = DateTime.Now;
            usuario.FechaActualizacion = DateTime.Now;

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Usuario registrado correctamente." });
        }

        // ✅ RECUPERAR CONTRASEÑA (solicitar token)
        [HttpPost("RECUPERAR-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == request.Correo);
            if (usuario == null)
                return NotFound("No existe un usuario con ese correo.");

            var token = Guid.NewGuid().ToString();
            usuario.TokenRecuperacion = token;
            usuario.FechaTokenExpira = DateTime.Now.AddMinutes(30);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Token de recuperación generado.",
                Token = token,
                Expira = usuario.FechaTokenExpira
            });
        }

        // ✅ RESETEAR CONTRASEÑA (con token)
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u =>
                u.TokenRecuperacion == request.Token &&
                u.FechaTokenExpira > DateTime.Now);

            if (usuario == null)
                return BadRequest("Token inválido o expirado.");

            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(request.NuevaContraseña);
            usuario.TokenRecuperacion = null;
            usuario.FechaTokenExpira = null;
            usuario.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok("Contraseña actualizada correctamente.");
        }

        // ✅ GENERAR TOKEN JWT
        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Correo),
                new Claim("idUsuario", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("nombre", usuario.Nombre)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(6),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ HASH Y VALIDACIÓN
        private bool VerificarContraseña(string input, string stored)
        {
            // Si es BCrypt (comienza con $2a o $2b)
            if (stored.StartsWith("$2a$") || stored.StartsWith("$2b$"))
            {
                return BCrypt.Net.BCrypt.Verify(input, stored);
            }

            // Si es SHA256
            using var sha = SHA256.Create();
            var hashInput = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
            return stored == hashInput;
        }

        // ✅ Modelos auxiliares
        public class LoginRequest
        {
            public string Correo { get; set; }
            public string Contraseña { get; set; }
        }

        public class ForgotPasswordRequest
        {
            public string Correo { get; set; }
        }

        public class ResetPasswordRequest
        {
            public string Token { get; set; }
            public string NuevaContraseña { get; set; }
        }
    }
}
