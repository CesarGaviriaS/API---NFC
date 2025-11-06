using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_NFC.Data;
using API___NFC.Models;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagAsignadoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TagAsignadoController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public async Task<IActionResult> PostTag([FromBody] TagAsignado tag)
        {
            if (string.IsNullOrWhiteSpace(tag.CodigoTag))
                return BadRequest("El código del tag es obligatorio.");

            // Evitar duplicado
            if (await _context.TagAsignado.AnyAsync(t => t.CodigoTag == tag.CodigoTag))
                return Conflict("Este tag ya está asignado.");

            tag.FechaAsignacion = DateTime.Now;
            _context.TagAsignado.Add(tag);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tag registrado correctamente.", tag });
        }

        [HttpGet("{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var tag = await _context.TagAsignado.FirstOrDefaultAsync(t => t.CodigoTag == codigo);
            if (tag == null)
                return NotFound("Tag no registrado.");
            return Ok(tag);
        }
    }
}
