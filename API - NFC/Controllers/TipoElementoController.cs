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
    public class TipoElementosController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public TipoElementosController(NfcDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoElemento>>> GetAll()
        {
            return await _context.TipoElementos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoElemento>> Get(int id)
        {
            var item = await _context.TipoElementos.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<TipoElemento>> Create(TipoElemento te)
        {
            _context.TipoElementos.Add(te);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = te.IdTipoElemento }, te);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TipoElemento te)
        {
            if (id != te.IdTipoElemento) return BadRequest();
            _context.Entry(te).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TipoElementos.AnyAsync(x => x.IdTipoElemento == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TipoElementos.FindAsync(id);
            if (item == null) return NotFound();
            _context.TipoElementos.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}