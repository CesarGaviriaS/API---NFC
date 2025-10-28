using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiNfc.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using API___NFC.Models;
using API___NFC.Models.Entity.Users;

namespace ApiNfc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public UsuariosController(NfcDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            // Get Aprendices
            var aprendices = await _context.Aprendices
                .Include(a => a.Ficha)
                .ThenInclude(f => f.Programa)
                .Where(a => a.Estado == true)
                .ToListAsync();

            // Get Funcionarios
            var funcionarios = await _context.Funcionarios
                .Where(f => f.Estado == true)
                .ToListAsync();

            // Create unified list
            var unifiedList = new List<object>();

            foreach (var aprendiz in aprendices)
            {
                unifiedList.Add(new
                {
                    IdUsuario = aprendiz.IdAprendiz,
                    Rol = "Aprendiz",
                    Aprendiz = new
                    {
                        IdAprendiz = aprendiz.IdAprendiz,
                        Nombre = aprendiz.Nombre,
                        Apellido = aprendiz.Apellido,
                        TipoDocumento = aprendiz.TipoDocumento,
                        NumeroDocumento = aprendiz.NumeroDocumento,
                        Documento = aprendiz.NumeroDocumento,
                        Correo = aprendiz.Correo,
                        CodigoBarras = aprendiz.CodigoBarras,
                        Telefono = aprendiz.Telefono,
                        IdFicha = aprendiz.IdFicha,
                        Ficha = aprendiz.Ficha != null ? new
                        {
                            IdFicha = aprendiz.Ficha.IdFicha,
                            Codigo = aprendiz.Ficha.Codigo,
                            Programa = aprendiz.Ficha.Programa != null ? new
                            {
                                NombrePrograma = aprendiz.Ficha.Programa.NombrePrograma
                            } : null
                        } : null
                    }
                });
            }

            foreach (var funcionario in funcionarios)
            {
                unifiedList.Add(new
                {
                    IdUsuario = funcionario.IdFuncionario,
                    Rol = "Funcionario",
                    Funcionario = new
                    {
                        IdFuncionario = funcionario.IdFuncionario,
                        Nombre = funcionario.Nombre,
                        Documento = funcionario.Documento,
                        Detalle = funcionario.Detalle
                    }
                });
            }

            return Ok(unifiedList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var item = await _context.Usuarios.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> Create(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario) return BadRequest();
            _context.Entry(usuario).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Usuarios.AnyAsync(u => u.IdUsuario == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Usuarios.FindAsync(id);
            if (item == null) return NotFound();
            _context.Usuarios.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        // Método paginated unificado (Aprendices y Funcionarios)
        [HttpGet("paginated")]
        public async Task<ActionResult<object>> GetUsuariosPaginated(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "")
        {
            // Get Aprendices
            var aprendices = await _context.Aprendices
                .Include(a => a.Ficha)
                .ThenInclude(f => f.Programa)
                .Where(a => a.Estado == true)
                .ToListAsync();

            // Get Funcionarios
            var funcionarios = await _context.Funcionarios
                .Where(f => f.Estado == true)
                .ToListAsync();

            // Create unified list
            var unifiedList = new List<object>();

            foreach (var aprendiz in aprendices)
            {
                unifiedList.Add(new
                {
                    IdUsuario = aprendiz.IdAprendiz,
                    Rol = "Aprendiz",
                    Aprendiz = new
                    {
                        IdAprendiz = aprendiz.IdAprendiz,
                        Nombre = aprendiz.Nombre,
                        Apellido = aprendiz.Apellido,
                        TipoDocumento = aprendiz.TipoDocumento,
                        NumeroDocumento = aprendiz.NumeroDocumento,
                        Documento = aprendiz.NumeroDocumento,
                        Correo = aprendiz.Correo,
                        CodigoBarras = aprendiz.CodigoBarras,
                        Telefono = aprendiz.Telefono,
                        IdFicha = aprendiz.IdFicha,
                        Ficha = aprendiz.Ficha != null ? new
                        {
                            IdFicha = aprendiz.Ficha.IdFicha,
                            Codigo = aprendiz.Ficha.Codigo,
                            Programa = aprendiz.Ficha.Programa != null ? new
                            {
                                NombrePrograma = aprendiz.Ficha.Programa.NombrePrograma
                            } : null
                        } : null
                    }
                });
            }

            foreach (var funcionario in funcionarios)
            {
                unifiedList.Add(new
                {
                    IdUsuario = funcionario.IdFuncionario,
                    Rol = "Funcionario",
                    Funcionario = new
                    {
                        IdFuncionario = funcionario.IdFuncionario,
                        Nombre = funcionario.Nombre,
                        Documento = funcionario.Documento,
                        Detalle = funcionario.Detalle
                    }
                });
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                unifiedList = unifiedList.Where(u =>
                {
                    var obj = u as dynamic;
                    if (obj.Rol == "Aprendiz")
                    {
                        var aprendiz = obj.Aprendiz;
                        return (aprendiz.Nombre?.ToString()?.ToLower()?.Contains(search) ?? false) ||
                               (aprendiz.Apellido?.ToString()?.ToLower()?.Contains(search) ?? false) ||
                               (aprendiz.NumeroDocumento?.ToString()?.Contains(search) ?? false);
                    }
                    else
                    {
                        var func = obj.Funcionario;
                        return (func.Nombre?.ToString()?.ToLower()?.Contains(search) ?? false) ||
                               (func.Documento?.ToString()?.Contains(search) ?? false);
                    }
                }).ToList();
            }

            var totalRecords = unifiedList.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedData = unifiedList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new
            {
                Data = paginatedData,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }
    }
}