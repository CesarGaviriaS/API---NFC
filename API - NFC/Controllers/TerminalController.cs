using API___NFC.Data;
using API___NFC.Models.Entity; // Asegúrate que este using apunte a tus modelos
using API___NFC.Models.Entity.Inventario;
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
                .Include(u => u.Funcionario)
                .Include(u => u.Aprendiz)
                    .ThenInclude(a => a.Ficha)
                        .ThenInclude(f => f.Programa)
                .Include(u => u.Elementos)
                    .ThenInclude(e => e.TipoElemento)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.Estado);

            if (usuario == null)
            {
                return NotFound(new { mensaje = $"Usuario con ID {idUsuario} no encontrado o está inactivo." });
            }

            object userData;

            if (usuario.Funcionario != null)
            {
                userData = new
                {
                    usuario.IdUsuario,
                    Nombre = usuario.Funcionario.Nombre,
                    Rol = "Funcionario",
                    Documento = usuario.Funcionario.Documento,
                    Ficha = (string)null,
                    Programa = (string)null,
                    NivelFormacion = (string)null,
                    Elementos = usuario.Elementos.Where(e => e.Estado).Select(MapElementoData).ToList()
                };
            }
            else if (usuario.Aprendiz != null)
            {
                userData = new
                {
                    usuario.IdUsuario,
                    Nombre = usuario.Aprendiz.Nombre,
                    Rol = "Aprendiz",
                    Documento = usuario.Aprendiz.Documento,
                    Ficha = usuario.Aprendiz.Ficha?.Codigo,
                    Programa = usuario.Aprendiz.Ficha?.Programa?.NombrePrograma,
                    NivelFormacion = usuario.Aprendiz.Ficha?.Programa?.NivelFormacion,
                    // --- CORRECCIÓN: Filtrar elementos por Estado = true ---
                    Elementos = usuario.Elementos.Where(e => e.Estado).Select(MapElementoData).ToList()
                };
            }
            else
            {
                return NotFound(new { mensaje = "El usuario no tiene un rol de Aprendiz o Funcionario asignado." });
            }

            return Ok(userData);
        }

        private object MapElementoData(Elemento e)
        {
            return new
            {
                e.IdElemento,
                e.NombreElemento,
                e.Serial,
                e.Marca,
                e.ImageUrl,
                e.CaracteristicasTecnicas,
                e.CaracteristicasFisicas,
                e.Detalles,
                Tipo = e.TipoElemento?.NombreTipoElemento ?? "Sin tipo"
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

