using API___NFC.Data;
using API___NFC.Models.Entity.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API___NFC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Try to find as Usuario first
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.NumeroDocumento == request.Documento && u.Estado);

                if (usuario != null)
                {
                    // Verificar contraseña (deberías usar hash en producción)
                    if (usuario.Contraseña != request.Contraseña)
                    {
                        return Unauthorized(new { message = "Contraseña incorrecta" });
                    }

                    // Generar token JWT
                    var userToken = GenerateJwtTokenUsuario(usuario);

                    return Ok(new
                    {
                        Token = userToken,
                        User = new
                        {
                            Id = usuario.IdUsuario,
                            Documento = usuario.NumeroDocumento,
                            Nombre = usuario.Nombre,
                            Apellido = usuario.Apellido,
                            Tipo = "Usuario",
                            Rol = usuario.Rol
                        }
                    });
                }

                // Fallback: Try Funcionario (legacy)
                var funcionario = await _context.Funcionarios
                    .FirstOrDefaultAsync(f => f.Documento == request.Documento && f.Estado);

                if (funcionario == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado o inactivo" });
                }

                // Verificar contraseña
                if (funcionario.Contraseña != request.Contraseña)
                {
                    return Unauthorized(new { message = "Contraseña incorrecta" });
                }

                // Generar token JWT
                var funcToken = GenerateJwtToken(funcionario);

                return Ok(new
                {
                    Token = funcToken,
                    User = new
                    {
                        Id = funcionario.IdFuncionario,
                        Documento = funcionario.Documento,
                        Nombre = funcionario.Nombre,
                        Tipo = "Funcionario"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        private string GenerateJwtToken(Funcionario funcionario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var keyString = jwtSettings["Key"];

            if (string.IsNullOrEmpty(keyString))
            {
                throw new Exception("JWT Key no está configurado");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, funcionario.IdFuncionario.ToString()),
        new Claim("TipoUsuario", "Funcionario"),
        new Claim("Documento", funcionario.Documento ?? "sin-documento")
    };

            // Manejar nombre nulo
            if (!string.IsNullOrEmpty(funcionario.Nombre))
            {
                claims.Add(new Claim(ClaimTypes.Name, funcionario.Nombre));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "DefaultIssuer",
                audience: jwtSettings["Audience"] ?? "DefaultAudience",
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateJwtTokenUsuario(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var keyString = jwtSettings["Key"];

            if (string.IsNullOrEmpty(keyString))
            {
                throw new Exception("JWT Key no está configurado");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim("TipoUsuario", "Usuario"),
                new Claim("Rol", usuario.Rol),
                new Claim("Documento", usuario.NumeroDocumento),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "DefaultIssuer",
                audience: jwtSettings["Audience"] ?? "DefaultAudience",
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class LoginRequest
        {
            public string Documento { get; set; }
            public string Contraseña { get; set; }
        }
    }
}