using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiNfc.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using API___NFC.Models;

namespace ApiNfc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FichasController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public FichasController(NfcDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ficha>>> GetAll()
        {
            return await _context.Fichas.Include(f => f.Programa).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Ficha>> GetFicha(int id)
        {
            var item = await _context.Fichas.Include(f => f.Programa).FirstOrDefaultAsync(f => f.IdFicha == id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Ficha>> Create(Ficha ficha)
        {
            _context.Fichas.Add(ficha);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFicha), new { id = ficha.IdFicha }, ficha);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Ficha ficha)
        {
            if (id != ficha.IdFicha) return BadRequest();
            _context.Entry(ficha).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Fichas.AnyAsync(f => f.IdFicha == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Fichas.FindAsync(id);
            if (item == null) return NotFound();
            _context.Fichas.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        // Método paginated (extraído del archivo)
        [HttpGet("paginated")]
        public async Task<ActionResult<object>> GetFichaPaginated(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "")
        {
            var query = _context.Fichas
                .Include(f => f.Programa)
                .Where(f => f.Estado == true);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f =>
                    (!string.IsNullOrEmpty(f.Codigo) && f.Codigo.Contains(search)) ||
                    (f.Programa != null && !string.IsNullOrEmpty(f.Programa.NombrePrograma) && f.Programa.NombrePrograma.Contains(search))
                );
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var fichas = await query
                .OrderBy(f => f.IdFicha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new
            {
                Data = fichas,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }
    }
}