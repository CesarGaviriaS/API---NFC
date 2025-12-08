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
    public class DetalleRegistroNFCController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DetalleRegistroNFCController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtener todos los detalles de un registro específico
        /// GET: api/DetalleRegistroNFC/porRegistro/{idRegistro}
        /// </summary>
        [HttpGet("porRegistro/{idRegistro}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDetallesPorRegistro(int idRegistro)
        {
            var detalles = await _context.DetalleRegistroNFC
                .Where(d => d.IdRegistroNFC == idRegistro)
                .Include(d => d.Elemento)
                    .ThenInclude(e => e.TipoElemento)
                .OrderBy(d => d.FechaHora)
                .Select(d => new
                {
                    d.IdDetalleRegistro,
                    d.IdElemento,
                    d.Accion,
                    d.FechaHora,
                    d.Validado,
                    Elemento = new
                    {
                        d.Elemento.Serial,
                        d.Elemento.Marca,
                        d.Elemento.Modelo,
                        TipoElemento = d.Elemento.TipoElemento.Tipo
                    }
                })
                .ToListAsync();

            return Ok(detalles);
        }

        /// <summary>
        /// Obtener el historial completo de un elemento/dispositivo
        /// GET: api/DetalleRegistroNFC/porElemento/{idElemento}
        /// </summary>
        [HttpGet("porElemento/{idElemento}")]
        public async Task<ActionResult<IEnumerable<object>>> GetHistorialElemento(int idElemento)
        {
            var historial =await _context.DetalleRegistroNFC
                .Where(d => d.IdElemento == idElemento)
                .Include(d => d.RegistroNFC)
                .Include(d => d.Proceso)
                .OrderByDescending(d => d.FechaHora)
                .Select(d => new
                {
                    d.IdDetalleRegistro,
                    d.Accion,
                    d.FechaHora,
                    d.Validado,
                    Registro = new
                    {
                        IdRegistro = d.RegistroNFC.IdRegistro,
                        d.RegistroNFC.TipoRegistro,
                        d.RegistroNFC.FechaRegistro
                    },
                    Proceso = new
                    {
                        IdProceso = d.Proceso.IdProceso,
                        d.Proceso.EstadoProceso,
                        d.Proceso.TipoPersona
                    }
                })
                .ToListAsync();

            return Ok(historial);
        }

        /// <summary>
        /// Obtener historial de un elemento por su serial
        /// GET: api/DetalleRegistroNFC/porSerial/{serial}
        /// </summary>
        [HttpGet("porSerial/{serial}")]
        public async Task<ActionResult<IEnumerable<object>>> GetHistorialPorSerial(string serial)
        {
            var elemento = await _context.Elemento
                .FirstOrDefaultAsync(e => e.Serial == serial);

            if (elemento == null)
            {
                return NotFound(new { mensaje = "Elemento no encontrado con ese serial" });
            }

            var historial = await _context.DetalleRegistroNFC
                .Where(d => d.IdElemento == elemento.IdElemento)
                .Include(d => d.RegistroNFC)
                .Include(d => d.Proceso)
                .Include(d => d.Elemento)
                    .ThenInclude(e => e.TipoElemento)
                .OrderByDescending(d => d.FechaHora)
                .Select(d => new
                {
                    d.IdDetalleRegistro,
                    d.Accion,
                    d.FechaHora,
                    d.Validado,
                    Elemento = new
                    {
                        d.Elemento.IdElemento,
                        d.Elemento.Serial,
                        d.Elemento.Marca,
                        d.Elemento.Modelo,
                        TipoElemento = d.Elemento.TipoElemento.Tipo
                    },
                    Registro = new
                    {
                        IdRegistro = d.RegistroNFC.IdRegistro,
                        d.RegistroNFC.TipoRegistro,
                        d.RegistroNFC.FechaRegistro
                    },
                    Proceso = new
                    {
                        IdProceso = d.Proceso.IdProceso,
                        d.Proceso.EstadoProceso
                    }
                })
                .ToListAsync();

            return Ok(historial);
        }

        /// <summary>
        /// Obtener detalles de un proceso específico
        /// GET: api/DetalleRegistroNFC/porProceso/{idProceso}
        /// </summary>
        [HttpGet("porProceso/{idProceso}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDetallesPorProceso(int idProceso)
        {
            var detalles = await _context.DetalleRegistroNFC
                .Where(d => d.IdProceso == idProceso)
                .Include(d => d.Elemento)
                    .ThenInclude(e => e.TipoElemento)
                .Include(d => d.RegistroNFC)
                .OrderBy(d => d.FechaHora)
                .Select(d => new
                {
                    d.IdDetalleRegistro,
                    d.Accion,
                    d.FechaHora,
                    d.Validado,
                    Elemento = new
                    {
                        d.Elemento.IdElemento,
                        d.Elemento.Serial,
                        d.Elemento.Marca,
                        d.Elemento.Modelo,
                        TipoElemento = d.Elemento.TipoElemento.Tipo
                    },
                    TipoRegistro = d.RegistroNFC.TipoRegistro
                })
                .ToListAsync();

            return Ok(detalles);
        }

        /// <summary>
        /// Estadísticas de un elemento
        /// GET: api/DetalleRegistroNFC/estadisticas/{idElemento}
        /// </summary>
        [HttpGet("estadisticas/{idElemento}")]
        public async Task<ActionResult<object>> GetEstadisticasElemento(int idElemento)
        {
            var detalles = await _context.DetalleRegistroNFC
                .Where(d => d.IdElemento == idElemento)
                .ToListAsync();

            if (!detalles.Any())
            {
                return NotFound(new { mensaje = "No hay historial para este elemento" });
            }

            var stats = new
            {
                TotalRegistros = detalles.Count,
                Ingresos = detalles.Count(d => d.Accion == "Ingresó"),
                Salidas = detalles.Count(d => d.Accion == "Salió"),
                VecesQuedo = detalles.Count(d => d.Accion == "Quedó"),
                PrimerRegistro = detalles.Min(d => d.FechaHora),
                UltimoRegistro = detalles.Max(d => d.FechaHora),
                UltimaAccion = detalles.OrderByDescending(d => d.FechaHora).First().Accion
            };

            return Ok(stats);
        }
    }
}
