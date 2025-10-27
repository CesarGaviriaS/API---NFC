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
                // Buscar funcionario por documento
                var funcionario = await _context.Funcionarios
                    .FirstOrDefaultAsync(f => f.Documento == request.Documento && f.Estado);

                if (funcionario == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado o inactivo" });
                }

                // Verificar contraseña (deberías usar hash en producción)
                if (funcionario.Contraseña != request.Contraseña)
                {
                    return Unauthorized(new { message = "Contraseña incorrecta" });
                }

                // Buscar usuario relacionado
                var usuario = await _context.Usuarios
                    .Include(u => u.Funcionario)
                    .FirstOrDefaultAsync(u => u.IdFuncionario == funcionario.IdFuncionario && u.Estado);

                if (usuario == null)
                {
                    return Unauthorized(new { message = "Usuario no configurado correctamente" });
                }

                // Generar token JWT
                var token = GenerateJwtToken(funcionario);

                return Ok(new
                {
                    Token = token,
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

        public class LoginRequest
        {
            public string Documento { get; set; }
            public string Contraseña { get; set; }
        }
    }
}