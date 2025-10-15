using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Academico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProgramaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/programa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Programa>>> GetProgramas()
        {
            return await _context.Programas.Where(p => p.Estado == true).ToListAsync();
        }

        // GET: api/programa/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Programa>> GetPrograma(int id)
        {
            var programa = await _context.Programas.FindAsync(id);
            if (programa == null || !programa.Estado)
            {
                return NotFound();
            }
            return programa;
        }

        // POST: api/programa
        [HttpPost]
        public async Task<ActionResult<Programa>> PostPrograma(Programa programa)
        {
            programa.Estado = true;
            _context.Programas.Add(programa);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPrograma), new { id = programa.IdPrograma }, programa);
        }

        // PUT: api/programa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrograma(int id, Programa programa)
        {
            if (id != programa.IdPrograma)
            {
                return BadRequest();
            }
            _context.Entry(programa).State = EntityState.Modified;
            _context.Entry(programa).Property(x => x.Estado).IsModified = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/programa/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrograma(int id)
        {
            var programa = await _context.Programas.FindAsync(id);
            if (programa == null)
            {
                return NotFound();
            }
            programa.Estado = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}