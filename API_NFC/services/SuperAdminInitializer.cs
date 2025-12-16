using API___NFC.Models;
using API_NFC.Data;
using Microsoft.EntityFrameworkCore;
using BCryptNet = BCrypt.Net.BCrypt;

namespace API___NFC.Services
{
    public class SuperAdminInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SuperAdminInitializer> _logger;

        public SuperAdminInitializer(ApplicationDbContext context, ILogger<SuperAdminInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Credenciales del super admin
                const string superAdminEmail = "admin@gmail.com";
                const string superAdminPassword = "admin12345";

                // Verificar si ya existe el super admin
                var existingSuperAdmin = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.Correo == superAdminEmail);

                if (existingSuperAdmin != null)
                {
                    _logger.LogInformation("Super admin ya existe en la base de datos.");
                    return;
                }

                // Crear el super admin
                var superAdmin = new Usuario
                {
                    Nombre = "Super",
                    Apellido = "Administrador",
                    TipoDocumento = "CC",
                    NumeroDocumento = "0000000000", // Documento único para el super admin
                    Correo = superAdminEmail,
                    Contraseña = BCryptNet.HashPassword(superAdminPassword),
                    Rol = "Administrador",
                    CodigoBarras = null,
                    Cargo = "Super Administrador",
                    Telefono = null,
                    FotoUrl = null,
                    Estado = true,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };

                _context.Usuario.Add(superAdmin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Super admin creado exitosamente con correo: {Email}", superAdminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al inicializar el super admin");
                // No lanzar la excepción para que la aplicación continúe ejecutándose
            }
        }
    }
}
