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
    public class TipoProcesosController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public TipoProcesosController(NfcDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoProceso>>> GetAll()
        {
            return await _context.TipoProcesos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoProceso>> Get(int id)
        {
            var item = await _context.TipoProcesos.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<TipoProceso>> Create(TipoProceso tp)
        {
            _context.TipoProcesos.Add(tp);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = tp.IdTipoProceso }, tp);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TipoProceso tp)
        {
            if (id != tp.IdTipoProceso) return BadRequest();
            _context.Entry(tp).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TipoProcesos.AnyAsync(x => x.IdTipoProceso == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TipoProcesos.FindAsync(id);
            if (item == null) return NotFound();
            _context.TipoProcesos.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}