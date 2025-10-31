﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_NFC.Data;
using API___NFC.Models;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElementoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ElementoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Elementoes (solo activos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Elemento>>> GetElemento()
        {
            return await _context.Elemento
                .Where(e => e.Estado == true)
                .Include(e => e.TipoElemento)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ GET: api/Elementoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Elemento>> GetElemento(int id)
        {
            var elemento = await _context.Elemento
                .Include(e => e.TipoElemento)
                .FirstOrDefaultAsync(e => e.IdElemento == id);

            if (elemento == null)
                return NotFound("No se encontró el elemento.");

            return Ok(elemento);
        }

        // ✅ GET: api/Elementoes/byNFC/{codigo}
        // 🔹 Este endpoint será usado por el agente NFC para buscar por tag
        [HttpGet("byNFC/{codigo}")]
        public async Task<ActionResult<Elemento>> GetByCodigoNFC(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
                return BadRequest("El código NFC no puede estar vacío.");

            var elemento = await _context.Elemento
                .Include(e => e.TipoElemento)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.CodigoNFC == codigo);

            if (elemento == null)
                return NotFound("No se encontró un elemento con ese código NFC.");

            return Ok(elemento);
        }

        // ✅ PUT: api/Elementoes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutElemento(int id, [FromBody] Elemento elemento)
        {
            ModelState.Remove(nameof(Elemento.TipoElemento));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Elemento.FindAsync(id);
            if (existing == null)
                return NotFound();

            // 🔹 Validar duplicados antes de actualizar
            if (await _context.Elemento.AnyAsync(e => e.Serial == elemento.Serial && e.IdElemento != id))
                return Conflict("Ya existe otro elemento con ese número de serie.");

            if (!string.IsNullOrEmpty(elemento.CodigoNFC) &&
                await _context.Elemento.AnyAsync(e => e.CodigoNFC == elemento.CodigoNFC && e.IdElemento != id))
                return Conflict("Ya existe otro elemento con ese código NFC.");

            // 🔹 Actualizar campos
            existing.IdTipoElemento = elemento.IdTipoElemento;
            existing.IdPropietario = elemento.IdPropietario;
            existing.TipoPropietario = elemento.TipoPropietario;
            existing.Marca = elemento.Marca;
            existing.Modelo = elemento.Modelo;
            existing.Serial = elemento.Serial;
            existing.CodigoNFC = elemento.CodigoNFC;
            existing.Descripcion = elemento.Descripcion;
            existing.ImagenUrl = elemento.ImagenUrl;
            existing.Estado = elemento.Estado ?? true;
            existing.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ✅ POST: api/Elementoes
        [HttpPost]
        public async Task<ActionResult<Elemento>> PostElemento(Elemento elemento)
        {
            // 🔹 Validar duplicados antes de guardar
            if (await _context.Elemento.AnyAsync(e => e.Serial == elemento.Serial))
                return Conflict("Ya existe un elemento con ese número de serie.");

            if (!string.IsNullOrEmpty(elemento.CodigoNFC) &&
                await _context.Elemento.AnyAsync(e => e.CodigoNFC == elemento.CodigoNFC))
                return Conflict("Ya existe un elemento con ese código NFC.");

            elemento.FechaCreacion = DateTime.Now;
            elemento.FechaActualizacion = DateTime.Now;
            elemento.Estado ??= true;

            _context.Elemento.Add(elemento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetElemento), new { id = elemento.IdElemento }, elemento);
        }

        // ✅ DELETE (Soft Delete): api/Elementoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteElemento(int id)
        {
            var elemento = await _context.Elemento.FindAsync(id);
            if (elemento == null)
                return NotFound();

            elemento.Estado = false;
            elemento.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Paginación: api/Elementoes/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult> GetElementosPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            const int maxPageSize = 100;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var query = _context.Elemento
                .Where(e => e.Estado == true)
                .Include(e => e.TipoElemento)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderBy(e => e.IdElemento)
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

        private bool ElementoExists(int id)
        {
            return _context.Elemento.Any(e => e.IdElemento == id);
        }
        // DTOs
        public class BindNfcRequest { public string CodigoNFC { get; set; } = string.Empty; }
        public class ClearNfcRequest { public bool Confirmar { get; set; } = true; }

        // POST api/Elementoes/{id}/bind-nfc
        [HttpPost("{id}/bind-nfc")]
        public async Task<IActionResult> BindNfc(int id, [FromBody] BindNfcRequest body)
        {
            if (string.IsNullOrWhiteSpace(body.CodigoNFC))
                return BadRequest("CodigoNFC es obligatorio.");

            var elemento = await _context.Elemento.FindAsync(id);
            if (elemento == null) return NotFound("Elemento no encontrado.");

            // Verificar que ese NFC no esté ya usado por otro elemento
            var enUso = await _context.Elemento
                .AnyAsync(e => e.CodigoNFC == body.CodigoNFC && e.IdElemento != id);
            if (enUso) return Conflict("Ese CodigoNFC ya está asignado a otro elemento.");

            elemento.CodigoNFC = body.CodigoNFC;
            elemento.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "NFC asignado al elemento.", elemento.IdElemento, elemento.CodigoNFC });
        }

        // POST api/Elementoes/{id}/clear-nfc
        [HttpPost("{id}/clear-nfc")]
        public async Task<IActionResult> ClearNfc(int id, [FromBody] ClearNfcRequest body)
        {
            if (body == null || !body.Confirmar) return BadRequest("Confirmación requerida.");
            var elemento = await _context.Elemento.FindAsync(id);
            if (elemento == null) return NotFound("Elemento no encontrado.");

            elemento.CodigoNFC = null;
            elemento.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "NFC removido del elemento.", elemento.IdElemento });
        }

        // GET api/Elementoes/by-nfc/{codigo}
        [HttpGet("by-nfc/{codigo}")]
        public async Task<IActionResult> GetByNfc(string codigo)
        {
            var elemento = await _context.Elemento
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.CodigoNFC == codigo);
            if (elemento == null) return NotFound("No existe un elemento con ese CodigoNFC.");
            return Ok(elemento);
        }

    }
}
