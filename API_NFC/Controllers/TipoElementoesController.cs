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
    public class TipoElementoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TipoElementoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/TipoElementoes (solo activos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoElemento>>> GetTipoElemento()
        {
            return await _context.TipoElemento
                .Where(t => t.Estado == true)
                .AsNoTracking()
                .OrderBy(t => t.Tipo)
                .ToListAsync();
        }

        // ✅ GET: api/TipoElementoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoElemento>> GetTipoElemento(int id)
        {
            var tipoElemento = await _context.TipoElemento.FindAsync(id);

            if (tipoElemento == null)
                return NotFound("No se encontró el tipo de elemento.");

            return Ok(tipoElemento);
        }

        // ✅ PUT: api/TipoElementoes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoElemento(int id, [FromBody] TipoElemento tipoElemento)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.TipoElemento.FindAsync(id);
            if (existing == null)
                return NotFound();

            // 🔹 Validar duplicado
            if (await _context.TipoElemento.AnyAsync(t => t.Tipo == tipoElemento.Tipo && t.IdTipoElemento != id))
                return Conflict("Ya existe un tipo de elemento con ese nombre.");

            existing.Tipo = tipoElemento.Tipo;
            existing.RequiereNFC = tipoElemento.RequiereNFC;
            existing.Estado = tipoElemento.Estado ?? true;
            existing.FechaCreacion ??= DateTime.Now;
            existing.FechaCreacion ??= DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ✅ POST: api/TipoElementoes
        [HttpPost]
        public async Task<ActionResult<TipoElemento>> PostTipoElemento(TipoElemento tipoElemento)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.TipoElemento.AnyAsync(t => t.Tipo == tipoElemento.Tipo))
                return Conflict("Ya existe un tipo de elemento con ese nombre.");

            tipoElemento.Estado ??= true;
            tipoElemento.FechaCreacion = DateTime.Now;

            _context.TipoElemento.Add(tipoElemento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTipoElemento), new { id = tipoElemento.IdTipoElemento }, tipoElemento);
        }

        // ✅ DELETE (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoElemento(int id)
        {
            var tipoElemento = await _context.TipoElemento.FindAsync(id);
            if (tipoElemento == null)
                return NotFound();

            tipoElemento.Estado = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ GET paginado: api/TipoElementoes/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult> GetTiposPaginados([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            const int maxPageSize = 100;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var query = _context.TipoElemento
                .Where(t => t.Estado == true)
                .AsNoTracking()
                .OrderBy(t => t.Tipo);

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
