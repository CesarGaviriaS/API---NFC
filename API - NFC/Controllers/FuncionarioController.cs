using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FuncionarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/funcionario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios()
        {
            return await _context.Funcionarios.Where(f => f.Estado == true).ToListAsync();
        }

        // GET: api/funcionario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Funcionario>> GetFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario == null || !funcionario.Estado)
            {
                return NotFound();
            }

            return funcionario;
        }

        // POST: api/funcionario
        [HttpPost]
        public async Task<ActionResult<Funcionario>> PostFuncionario(Funcionario funcionario)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Validar modelo
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    // 1. Crear el Funcionario
                    funcionario.Estado = true;
                    _context.Funcionarios.Add(funcionario);
                    await _context.SaveChangesAsync();

                    // 2. Crear el Usuario
                    var nuevoUsuario = new Usuario
                    {
                        IdFuncionario = funcionario.IdFuncionario,
                        IdAprendiz = null,
                        Estado = true
                    };
                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(GetFuncionario), new { id = funcionario.IdFuncionario }, funcionario);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    return StatusCode(500, "Ocurrió un error interno al crear el funcionario y el usuario asociado.");
                }
            }
        }

        // PUT: api/funcionario/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFuncionario(int id, Funcionario funcionario)
        {
            // Validar modelo
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != funcionario.IdFuncionario)
            {
                return BadRequest();
            }

            // Buscar el funcionario existente
            var funcionarioExistente = await _context.Funcionarios.FindAsync(id);
            if (funcionarioExistente == null)
            {
                return NotFound();
            }

            // Actualizar propiedades
            funcionarioExistente.Nombre = funcionario.Nombre;
            funcionarioExistente.Documento = funcionario.Documento;
            funcionarioExistente.Detalle = funcionario.Detalle;

            // Solo actualizar contraseña si se proporciona y no está vacía
            if (!string.IsNullOrWhiteSpace(funcionario.Contraseña))
            {
                funcionarioExistente.Contraseña = funcionario.Contraseña;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FuncionarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/funcionario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null)
            {
                return NotFound();
            }

            // Buscar usuario asociado
            var usuarioAsociado = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdFuncionario == id);

            funcionario.Estado = false;
            if (usuarioAsociado != null)
            {
                usuarioAsociado.Estado = false;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool FuncionarioExists(int id)
        {
            return _context.Funcionarios.Any(e => e.IdFuncionario == id);
        }
    }
}