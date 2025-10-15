using API___NFC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace API___NFC.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GestionController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public GestionController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var usuarios = await _db.Usuarios
                .Where(u => u.Estado)
                .Select(u => new
                {
                    u.IdUsuario,
                    Nombre = u.Funcionario != null ? u.Funcionario.Nombre : u.Aprendiz.Nombre
                })
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("GetElementsByUser")]
        public async Task<IActionResult> GetElementsByUser(int idUsuario)
        {
            // --- CORRECCIÓN ---
            // La consulta ahora empieza desde la tabla de Usuarios para encontrar el
            // usuario correcto y luego navega a su colección de Elementos.
            // Esto respeta la relación definida en tus modelos y soluciona el error.
            var elementos = await _db.Usuarios
                .Where(u => u.IdUsuario == idUsuario && u.Estado) // 1. Encuentra al usuario correcto.
                .SelectMany(u => u.Elementos.Where(e => e.Estado)) // 2. Selecciona solo sus elementos activos.
                .Select(e => new
                {
                    e.IdElemento,
                    e.NombreElemento
                })
                .OrderBy(e => e.NombreElemento)
                .ToListAsync();

            return Ok(elementos);
        }
    }
}

