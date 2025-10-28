using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiNfc.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using API___NFC.Models.Entity.Users;

namespace ApiNfc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionarioController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public FuncionarioController(NfcDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetAll()
        {
            return await _context.Funcionarios.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Funcionario>> GetFuncionario(int id)
        {
            var item = await _context.Funcionarios.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Funcionario>> Create(Funcionario funcionario)
        {
            _context.Funcionarios.Add(funcionario);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFuncionario), new { id = funcionario.IdFuncionario }, funcionario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Funcionario funcionario)
        {
            if (id != funcionario.IdFuncionario) return BadRequest();
            _context.Entry(funcionario).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Funcionarios.AnyAsync(f => f.IdFuncionario == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Funcionarios.FindAsync(id);
            if (item == null) return NotFound();
            _context.Funcionarios.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
