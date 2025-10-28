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
    public class RegistrosNFCController : ControllerBase
    {
        private readonly NfcDbContext _context;

        public RegistrosNFCController(NfcDbContext context)
        {
            _context = context;
        }

        // GET: api/RegistrosNFC
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistroNFC>>> GetAll()
        {
            return await _context.RegistrosNFC
                                 .Include(r => r.Aprendiz)
                                 .Include(r => r.Usuario)
                                 .ToListAsync();
        }

        // GET: api/RegistrosNFC/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistroNFC>> GetRegistro(int id)
        {
            var item = await _context.RegistrosNFC
                                     .Include(r => r.Aprendiz)
                                     .Include(r => r.Usuario)
                                     .FirstOrDefaultAsync(r => r.IdRegistro == id);

            if (item == null) return NotFound();
            return item;
        }

        // POST: api/RegistrosNFC
        [HttpPost]
        public async Task<ActionResult<RegistroNFC>> Create(RegistroNFC registro)
        {
            _context.RegistrosNFC.Add(registro);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRegistro), new { id = registro.IdRegistro }, registro);
        }

        // PUT: api/RegistrosNFC/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RegistroNFC registro)
        {
            if (id != registro.IdRegistro) return BadRequest();

            _context.Entry(registro).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.RegistrosNFC.AnyAsync(r => r.IdRegistro == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/RegistrosNFC/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.RegistrosNFC.FindAsync(id);
            if (item == null) return NotFound();

            _context.RegistrosNFC.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}