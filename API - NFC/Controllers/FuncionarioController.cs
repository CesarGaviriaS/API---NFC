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
        // Obtiene todos los funcionarios activos.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios()
        {
            return await _context.Funcionarios.Where(f => f.Estado == true).ToListAsync();
        }

        // GET: api/funcionario/5
        // Obtiene un funcionario específico por su ID.
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
        // Crea un nuevo Funcionario y su Usuario correspondiente en una transacción.
        [HttpPost]
        public async Task<ActionResult<Funcionario>> PostFuncionario(Funcionario funcionario)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Crear el Funcionario
                    funcionario.Estado = true;
                    _context.Funcionarios.Add(funcionario);
                    await _context.SaveChangesAsync(); // Guardamos para obtener el ID del nuevo funcionario

                    // 2. Crear el Usuario que lo "envuelve"
                    var nuevoUsuario = new Usuario
                    {
                        IdFuncionario = funcionario.IdFuncionario, // Lo asociamos con el ID que acabamos de crear
                        IdAprendiz = null,
                        Estado = true
                    };
                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();

                    // Si todo sale bien, confirmamos la transacción
                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(GetFuncionario), new { id = funcionario.IdFuncionario }, funcionario);
                }
                catch (Exception)
                {
                    // Si algo falla, revertimos todos los cambios
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Ocurrió un error interno al crear el funcionario y el usuario asociado.");
                }
            }
        }

        // PUT: api/funcionario/5
        // Actualiza un funcionario existente.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFuncionario(int id, Funcionario funcionario)
        {
            if (id != funcionario.IdFuncionario)
            {
                return BadRequest();
            }

            _context.Entry(funcionario).State = EntityState.Modified;
            _context.Entry(funcionario).Property(x => x.Estado).IsModified = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/funcionario/5
        // Desactiva un funcionario Y su usuario asociado.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null)
            {
                return NotFound();
            }

            // Buscamos el usuario asociado para también desactivarlo
            var usuarioAsociado = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdFuncionario == id);

            funcionario.Estado = false;
            if (usuarioAsociado != null)
            {
                usuarioAsociado.Estado = false;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}