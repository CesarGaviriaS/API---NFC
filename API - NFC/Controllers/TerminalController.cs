using API___NFC.Data;
using API___NFC.Models.Constants;
using API___NFC.Models.Entity;
using API___NFC.Models.Entity.Inventario;
using API___NFC.Models.Entity.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace API___NFC.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TerminalController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TerminalController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserData(int idUsuario)
        {
            var usuario = await _db.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.Estado);

            if (usuario != null)
            {
                // Get elementos owned by this usuario
                var elementos = await _db.Elementos
                    .Include(e => e.TipoElemento)
                    .Where(e => e.IdPropietario == idUsuario && e.TipoPropietario == AppConstants.OwnerTypes.Usuario && e.Estado)
                    .ToListAsync();

                var userData = new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Apellido,
                    Rol = usuario.Rol,
                    Documento = usuario.NumeroDocumento,
                    usuario.Correo,
                    Cargo = usuario.Cargo,
                    Ficha = (string)null,
                    Programa = (string)null,
                    NivelFormacion = (string)null,
                    Elementos = elementos.Select(MapElementoData).ToList()
                };

                return Ok(userData);
            }

            // Try to find as Aprendiz
            var aprendiz = await _db.Aprendices
                .AsNoTracking()
                .Include(a => a.Ficha)
                    .ThenInclude(f => f.Programa)
                .FirstOrDefaultAsync(a => a.IdAprendiz == idUsuario && a.Estado);

            if (aprendiz != null)
            {
                // Get elementos owned by this aprendiz
                var elementos = await _db.Elementos
                    .Include(e => e.TipoElemento)
                    .Where(e => e.IdPropietario == aprendiz.IdAprendiz && e.TipoPropietario == AppConstants.OwnerTypes.Aprendiz && e.Estado)
                    .ToListAsync();

                var userData = new
                {
                    IdUsuario = aprendiz.IdAprendiz,
                    aprendiz.Nombre,
                    aprendiz.Apellido,
                    Rol = "Aprendiz",
                    Documento = aprendiz.NumeroDocumento,
                    aprendiz.Correo,
                    Cargo = (string)null,
                    Ficha = aprendiz.Ficha?.Codigo,
                    Programa = aprendiz.Ficha?.Programa?.NombrePrograma,
                    NivelFormacion = aprendiz.Ficha?.Programa?.NivelFormacion,
                    Elementos = elementos.Select(MapElementoData).ToList()
                };

                return Ok(userData);
            }

            return NotFound(new { mensaje = $"Usuario o Aprendiz con ID {idUsuario} no encontrado o está inactivo." });
        }

        private object MapElementoData(Elemento e)
        {
            return new
            {
                e.IdElemento,
                e.Marca,
                e.Modelo,
                e.Serial,
                ImageUrl = e.ImagenUrl,
                Descripcion = e.Descripcion,
                Tipo = e.TipoElemento?.Tipo ?? "Sin tipo"
            };
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarProceso([FromBody] ProcesoDto datos)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { exito = false, mensaje = "Los datos recibidos no son válidos." });

            // Lógica para guardar el proceso en la base de datos...

            return Ok(new { exito = true, mensaje = $"Proceso de {datos.TipoProceso.ToUpper()} finalizado con éxito para {datos.Elementos.Count} elemento(s)." });
        }
    }

    public class ProcesoDto
    {
        public int IdUsuario { get; set; }
        public List<int> Elementos { get; set; } = new();
        public string TipoProceso { get; set; }
    }
}

