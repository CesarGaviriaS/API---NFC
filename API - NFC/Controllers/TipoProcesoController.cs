using API___NFC.Data;
using API___NFC.Models;
using API___NFC.Models.Entity.Proceso;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoProcesoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TipoProcesoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/tipoproceso
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoProceso>>> GetTiposProceso()
        {
            return await _context.TiposProceso.Where(t => t.Estado == true).ToListAsync();
        }

        // GET: api/tipoproceso/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoProceso>> GetTipoProceso(int id)
        {
            var tipoProceso = await _context.TiposProceso.FindAsync(id);
            if (tipoProceso == null || !tipoProceso.Estado)
            {
                return NotFound();
            }
            return tipoProceso;
        }

        // POST: api/tipoproceso
        [HttpPost]
        public async Task<ActionResult<TipoProceso>> PostTipoProceso(TipoProceso tipoProceso)
        {
            tipoProceso.Estado = true;
            _context.TiposProceso.Add(tipoProceso);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTipoProceso), new { id = tipoProceso.IdTipoProceso }, tipoProceso);
        }

        // PUT: api/tipoproceso/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoProceso(int id, TipoProceso tipoProceso)
        {
            if (id != tipoProceso.IdTipoProceso)
            {
                return BadRequest();
            }
            _context.Entry(tipoProceso).State = EntityState.Modified;
            _context.Entry(tipoProceso).Property(x => x.Estado).IsModified = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tipoproceso/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoProceso(int id)
        {
            var tipoProceso = await _context.TiposProceso.FindAsync(id);
            if (tipoProceso == null)
            {
                return NotFound();
            }
            tipoProceso.Estado = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}