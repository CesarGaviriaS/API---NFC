using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // --- MÉTODO AÑADIDO ---
        // GET: api/usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Include(u => u.Aprendiz)
                    .ThenInclude(a => a.Ficha)
                .Include(u => u.Funcionario)
                .Where(u => u.Estado == true)
                .ToListAsync();
        }

        // GET: api/usuario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Aprendiz)
                    .ThenInclude(a => a.Ficha)
                .Include(u => u.Funcionario)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null || !usuario.Estado)
            {
                return NotFound();
            }
            return usuario;
        }

        // DELETE: api/usuario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // También desactivamos el aprendiz o funcionario asociado si existe
            if (usuario.IdAprendiz.HasValue)
            {
                var aprendiz = await _context.Aprendices.FindAsync(usuario.IdAprendiz.Value);
                if (aprendiz != null) aprendiz.Estado = false;
            }
            if (usuario.IdFuncionario.HasValue)
            {
                var funcionario = await _context.Funcionarios.FindAsync(usuario.IdFuncionario.Value);
                if (funcionario != null) funcionario.Estado = false;
            }

            usuario.Estado = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // NOTA: POST (Crear) y PUT (Actualizar) se manejan a través de
        // los controladores de Aprendiz y Funcionario.
    }
}