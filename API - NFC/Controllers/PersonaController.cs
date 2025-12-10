using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_NFC.Data;
using System;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PersonaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Persona/byDocumento/{documento}
        // 🎯 Busca automáticamente en Aprendiz y Usuario por NumeroDocumento EXACTO
        [HttpGet("byDocumento/{documento}")]
        public async Task<IActionResult> GetPersonaByDocumento(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
            {
                return BadRequest(new { Message = "Debe proporcionar un número de documento" });
            }

            Console.WriteLine($"🔍 Buscando documento: '{documento}'");

            // 1️⃣ Buscar en tabla Aprendiz (con includes de Ficha y Programa)
            var aprendiz = await _context.Aprendiz
                .Include(a => a.Ficha)
                    .ThenInclude(f => f.Programa)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.NumeroDocumento == documento && a.Estado == true);

            if (aprendiz != null)
            {
                Console.WriteLine($"✅ Aprendiz encontrado: {aprendiz.Nombre} {aprendiz.Apellido}");
                return Ok(new
                {
                    TipoPersona = "Aprendiz",
                    IdPersona = aprendiz.IdAprendiz,
                    Data = aprendiz
                });
            }

            Console.WriteLine("❌ No encontrado en Aprendiz, buscando en Usuario...");

            // 2️⃣ Buscar en tabla Usuario
            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.NumeroDocumento == documento && u.Estado == true);

            if (usuario != null)
            {
                Console.WriteLine($"✅ Usuario encontrado: {usuario.Nombre} {usuario.Apellido}");
                return Ok(new
                {
                    TipoPersona = "Usuario",
                    IdPersona = usuario.IdUsuario,
                    Data = usuario
                });
            }

            // 3️⃣ No encontrado en ninguna tabla
            Console.WriteLine($"❌ No encontrado en ninguna tabla con documento: '{documento}'");
            return NotFound(new { Message = $"No se encontró ninguna persona activa con el documento {documento}" });
        }

        // ✅ GET: api/Persona/search?q={query}
        // 🔍 Búsqueda DINÁMICA - busca por documento parcial, nombre o apellido
        [HttpGet("search")]
        public async Task<IActionResult> SearchPersonas([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Ok(new List<object>()); // Retorna lista vacía si query es muy corto
            }

            var query = q.Trim();
            var resultados = new List<object>();

            try
            {
                Console.WriteLine($"🔍 Búsqueda dinámica con query: '{query}'");

                // 🔍 Buscar en Aprendiz
                var aprendices = await _context.Aprendiz
                    .Include(a => a.Ficha)
                        .ThenInclude(f => f.Programa)
                    .AsNoTracking()
                    .Where(a => a.Estado == true &&
                        (EF.Functions.Like(a.NumeroDocumento, $"%{query}%") ||
                         EF.Functions.Like(a.Nombre, $"%{query}%") ||
                         EF.Functions.Like(a.Apellido, $"%{query}%")))
                    .Take(5)
                    .ToListAsync();

                Console.WriteLine($"  ✅ Encontrados {aprendices.Count} aprendices");

                foreach (var a in aprendices)
                {
                    resultados.Add(new
                    {
                        TipoPersona = "Aprendiz",
                        IdPersona = a.IdAprendiz,
                        NumeroDocumento = a.NumeroDocumento,
                        NombreCompleto = $"{a.Nombre} {a.Apellido}",
                        Correo = a.Correo,
                        Data = a
                    });
                }

                // 🔍 Buscar en Usuario
                var usuarios = await _context.Usuario
                    .AsNoTracking()
                    .Where(u => u.Estado == true &&
                        (EF.Functions.Like(u.NumeroDocumento, $"%{query}%") ||
                         EF.Functions.Like(u.Nombre, $"%{query}%") ||
                         EF.Functions.Like(u.Apellido, $"%{query}%")))
                    .Take(5)
                    .ToListAsync();

                Console.WriteLine($"  ✅ Encontrados {usuarios.Count} usuarios");

                foreach (var u in usuarios)
                {
                    resultados.Add(new
                    {
                        TipoPersona = "Usuario",
                        IdPersona = u.IdUsuario,
                        NumeroDocumento = u.NumeroDocumento,
                        NombreCompleto = $"{u.Nombre} {u.Apellido}",
                        Correo = u.Correo,
                        Rol = u.Rol,
                        Data = u
                    });
                }

                Console.WriteLine($"🔍 Total resultados: {resultados.Count}");
                return Ok(resultados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en búsqueda: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return Ok(new List<object>()); // Retorna lista vacía en caso de error
            }
        }
    }
}
