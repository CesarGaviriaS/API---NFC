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
    public class ElementoProcesosController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public ElementoProcesosController(NfcDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ElementoProceso>>> GetAll()
        {
            return await _context.ElementoProcesos.Include(ep => ep.Elemento).Include(ep => ep.Proceso).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ElementoProceso>> Get(int id)
        {
            var item = await _context.ElementoProcesos.Include(ep => ep.Elemento).Include(ep => ep.Proceso)
                .FirstOrDefaultAsync(ep => ep.IdElementoProceso == id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<ElementoProceso>> Create(ElementoProceso ep)
        {
            _context.ElementoProcesos.Add(ep);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = ep.IdElementoProceso }, ep);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ElementoProceso ep)
        {
            if (id != ep.IdElementoProceso) return BadRequest();
            _context.Entry(ep).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ElementoProcesos.AnyAsync(x => x.IdElementoProceso == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ElementoProcesos.FindAsync(id);
            if (item == null) return NotFound();
            _context.ElementoProcesos.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}