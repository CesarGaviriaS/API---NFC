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
    public class ProcesosController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public ProcesosController(NfcDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proceso>>> GetAll()
        {
            return await _context.Procesos
                .Include(p => p.TipoProceso)
                .Include(p => p.Aprendiz)
                .Include(p => p.Usuario)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Proceso>> GetProceso(int id)
        {
            var item = await _context.Procesos
                .Include(p => p.TipoProceso)
                .Include(p => p.Aprendiz)
                .Include(p => p.Usuario)
                .Include(p => p.ElementoProcesos)
                .FirstOrDefaultAsync(p => p.IdProceso == id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Proceso>> Create(Proceso proceso)
        {
            _context.Procesos.Add(proceso);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProceso), new { id = proceso.IdProceso }, proceso);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Proceso proceso)
        {
            if (id != proceso.IdProceso) return BadRequest();
            _context.Entry(proceso).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Procesos.AnyAsync(p => p.IdProceso == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Procesos.FindAsync(id);
            if (item == null) return NotFound();
            _context.Procesos.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}