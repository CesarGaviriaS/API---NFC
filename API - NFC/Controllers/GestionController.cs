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
                    u.Nombre,
                    u.Apellido
                })
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("GetElementsByUser")]
        public async Task<IActionResult> GetElementsByUser(int idUsuario)
        {
            // Get elementos owned by this usuario
            var elementos = await _db.Elementos
                .Where(e => e.IdPropietario == idUsuario && e.TipoPropietario == "Usuario" && e.Estado)
                .Select(e => new
                {
                    e.IdElemento,
                    e.Marca,
                    e.Modelo
                })
                .OrderBy(e => e.Marca)
                .ToListAsync();

            return Ok(elementos);
        }
    }
}

