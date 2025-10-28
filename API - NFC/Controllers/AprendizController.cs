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
    public class AprendicesController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public AprendicesController(NfcDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aprendiz>>> GetAll()
        {
            return await _context.Aprendices.Include(a => a.Ficha).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Aprendiz>> GetAprendiz(int id)
        {
            var item = await _context.Aprendices.Include(a => a.Ficha).FirstOrDefaultAsync(a => a.IdAprendiz == id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Aprendiz>> Create(Aprendiz aprendiz)
        {
            _context.Aprendices.Add(aprendiz);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAprendiz), new { id = aprendiz.IdAprendiz }, aprendiz);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Aprendiz aprendiz)
        {
            if (id != aprendiz.IdAprendiz) return BadRequest();
            _context.Entry(aprendiz).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Aprendices.AnyAsync(a => a.IdAprendiz == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Aprendices.FindAsync(id);
            if (item == null) return NotFound();
            _context.Aprendices.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}