using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using ApiNfc.Data;


namespace ApiNfc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElementosController : ControllerBase
    {
        private readonly NfcDbContext _context;
        public ElementosController(NfcDbContext context) => _context = context;

        // ... otros endpoints (GetAll, GetElemento, Create, Update, Delete) ...

        // GET: api/elementos/paginated
        [HttpGet("paginated")]
        public async Task<ActionResult<object>> GetElementosPaginated(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "")
        {
            var query = _context.Elementos
                .Include(e => e.TipoElemento)
                // .Include(e => e.Propietario).ThenInclude(p => p.Aprendiz)
                // .Include(e => e.Propietario).ThenInclude(p => p.Funcionario)
                .Where(e => e.Estado == true);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    (e.Marca != null && e.Marca.Contains(search)) ||
                    (e.Modelo != null && e.Modelo.Contains(search)) ||
                    (e.Serial != null && e.Serial.Contains(search)) ||
                    (e.CodigoNFC != null && e.CodigoNFC.Contains(search)) ||
                    (e.Descripcion != null && e.Descripcion.Contains(search))
                );
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var elementos = await query
                .OrderBy(e => e.IdElemento)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new
            {
                Data = elementos,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }
    }
}