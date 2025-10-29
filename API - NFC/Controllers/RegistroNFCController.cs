using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_NFC.Data;
using API___NFC.Models;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistroNFCController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegistroNFCController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RegistroNFC (solo activos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistroNFC>>> GetRegistros()
        {
            return await _context.RegistrosNFC
                .Where(r => r.Estado == null || r.Estado == "Activo")
                .Include(r => r.Usuario)
                .Include(r => r.Aprendiz)
                .AsNoTracking()
                .OrderByDescending(r => r.FechaRegistro)
                .ToListAsync();
        }

        // GET: api/RegistroNFC/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistroNFC>> GetRegistroNFC(int id)
        {
            var registro = await _context.RegistrosNFC
                .Include(r => r.Usuario)
                .Include(r => r.Aprendiz)
                .FirstOrDefaultAsync(r => r.IdRegistro == id);

            if (registro == null)
                return NotFound("Registro no encontrado.");

            return Ok(registro);
        }

        // POST: api/RegistroNFC
        [HttpPost]
        public async Task<ActionResult<RegistroNFC>> PostRegistroNFC([FromBody] RegistroNFC registro)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validaciones mínimas (FKs obligatorias y tipo)
            if (registro.IdAprendiz <= 0 || registro.IdUsuario <= 0)
                return BadRequest("IdAprendiz y IdUsuario son obligatorios.");

            if (string.IsNullOrWhiteSpace(registro.TipoRegistro))
                return BadRequest("TipoRegistro es obligatorio.");

            // Asegurar valores por defecto compatibles con la BD
            registro.FechaRegistro ??= DateTime.Now;
            registro.Estado ??= "Activo";

            _context.RegistrosNFC.Add(registro);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRegistroNFC), new { id = registro.IdRegistro }, registro);
        }

        // PUT: api/RegistroNFC/5 (opcional)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegistroNFC(int id, [FromBody] RegistroNFC registro)
        {
            var existing = await _context.RegistrosNFC.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Puedes permitir actualizar solo algunos campos
            if (!string.IsNullOrWhiteSpace(registro.TipoRegistro))
                existing.TipoRegistro = registro.TipoRegistro;

            if (!string.IsNullOrWhiteSpace(registro.Estado))
                existing.Estado = registro.Estado;

            // Si decides permitir cambiar usuarios/aprendiz, valida que existan:
            // existing.IdUsuario = registro.IdUsuario;
            // existing.IdAprendiz = registro.IdAprendiz;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/RegistroNFC/5 (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistroNFC(int id)
        {
            var registro = await _context.RegistrosNFC.FindAsync(id);
            if (registro == null)
                return NotFound();

            registro.Estado = "Inactivo";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/RegistroNFC/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult> GetRegistrosPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            const int maxPageSize = 100;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var query = _context.RegistrosNFC
                .Where(r => r.Estado == null || r.Estado == "Activo")
                .Include(r => r.Usuario)
                .Include(r => r.Aprendiz)
                .AsNoTracking()
                .OrderByDescending(r => r.FechaRegistro);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var metadata = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            Response.Headers["X-Pagination"] = System.Text.Json.JsonSerializer.Serialize(metadata);

            return Ok(new
            {
                Items = items,
                metadata.PageNumber,
                metadata.PageSize,
                metadata.TotalCount,
                metadata.TotalPages
            });
        }
    }
}
