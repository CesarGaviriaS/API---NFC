using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiNfc.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using API___NFC.Models;
using API___NFC.Models.Entity.Users;
using API___NFC.Models.Dto;

namespace ApiNfc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public UsuariosController(NfcDbContext context) => _context = context;

        // Helper method to create unified list
        private async Task<List<UnifiedUsuarioDto>> GetUnifiedUsuariosList()
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
            var unifiedList = new List<UnifiedUsuarioDto>();

            foreach (var aprendiz in aprendices)
            {
                unifiedList.Add(new UnifiedUsuarioDto
                {
                    IdUsuario = aprendiz.IdAprendiz,
                    Rol = "Aprendiz",
                    Aprendiz = new AprendizDto
                    {
                        IdAprendiz = aprendiz.IdAprendiz,
                        Nombre = aprendiz.Nombre,
                        Apellido = aprendiz.Apellido,
                        TipoDocumento = aprendiz.TipoDocumento,
                        NumeroDocumento = aprendiz.NumeroDocumento,
                        Correo = aprendiz.Correo,
                        CodigoBarras = aprendiz.CodigoBarras,
                        Telefono = aprendiz.Telefono,
                        IdFicha = aprendiz.IdFicha,
                        Ficha = aprendiz.Ficha != null ? new FichaSimpleDto
                        {
                            IdFicha = aprendiz.Ficha.IdFicha,
                            Codigo = aprendiz.Ficha.Codigo,
                            Programa = aprendiz.Ficha.Programa != null ? new ProgramaSimpleDto
                            {
                                NombrePrograma = aprendiz.Ficha.Programa.NombrePrograma
                            } : null
                        } : null
                    }
                });
            }

            foreach (var funcionario in funcionarios)
            {
                unifiedList.Add(new UnifiedUsuarioDto
                {
                    IdUsuario = funcionario.IdFuncionario,
                    Rol = "Funcionario",
                    Funcionario = new FuncionarioDto
                    {
                        IdFuncionario = funcionario.IdFuncionario,
                        Nombre = funcionario.Nombre,
                        Documento = funcionario.Documento,
                        Detalle = funcionario.Detalle
                    }
                });
            }

            return unifiedList;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnifiedUsuarioDto>>> GetAll()
        {
            var unifiedList = await GetUnifiedUsuariosList();
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
            // Get unified list
            var unifiedList = await GetUnifiedUsuariosList();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                unifiedList = unifiedList.Where(u =>
                {
                    if (u.Rol == "Aprendiz" && u.Aprendiz != null)
                    {
                        return (u.Aprendiz.Nombre?.ToLower()?.Contains(search) ?? false) ||
                               (u.Aprendiz.Apellido?.ToLower()?.Contains(search) ?? false) ||
                               (u.Aprendiz.NumeroDocumento?.Contains(search) ?? false);
                    }
                    else if (u.Rol == "Funcionario" && u.Funcionario != null)
                    {
                        return (u.Funcionario.Nombre?.ToLower()?.Contains(search) ?? false) ||
                               (u.Funcionario.Documento?.Contains(search) ?? false);
                    }
                    return false;
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