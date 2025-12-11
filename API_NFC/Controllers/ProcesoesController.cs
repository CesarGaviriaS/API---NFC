using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_NFC.Data;
using API___NFC.Models;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Presentation;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcesoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProcesoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Procesoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proceso>>> GetProcesos()
        {
            var procesos = await _context.Proceso
                .Include(p => p.TipoProceso)
                .Include(p => p.Usuario)
                .Include(p => p.Aprendiz)
                .AsNoTracking()
                .ToListAsync();

            return Ok(procesos);
        }

        // ✅ GET: api/Procesoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Proceso>> GetProceso(int id)
        {
            var proceso = await _context.Proceso
                .Include(p => p.TipoProceso)
                .Include(p => p.Usuario)
                .Include(p => p.Aprendiz)
                .FirstOrDefaultAsync(p => p.IdProceso == id);

            if (proceso == null)
                return NotFound(new { message = "Proceso no encontrado." });

            return Ok(proceso);
        }

        // GET: api/Procesoes/activo/{tipoPersona}/{idPersona}
        [HttpGet("activo/{tipoPersona}/{idPersona}")]
        public async Task<ActionResult<object>> GetProcesoActivo(string tipoPersona, int idPersona)
        {
            Console.WriteLine($"🔍 Buscando proceso activo para {tipoPersona} ID: {idPersona}");

            if (tipoPersona != "Usuario" && tipoPersona != "Aprendiz")
            {
                return BadRequest(new { message = "TipoPersona debe ser 'Usuario' o 'Aprendiz'." });
            }

            try
            {
                Proceso procesoActivo = null;

                if (tipoPersona == "Aprendiz")
                {
                    procesoActivo = await _context.Proceso
                        .Where(p => p.IdAprendiz == idPersona &&
                                   p.TipoPersona == "Aprendiz" &&
                                   (p.EstadoProceso == "Abierto" || p.EstadoProceso == "EnCurso"))
                        .Include(p => p.TipoProceso)
                        .OrderByDescending(p => p.IdProceso)
                        .FirstOrDefaultAsync();
                }
                else if (tipoPersona == "Usuario")
                {
                    procesoActivo = await _context.Proceso
                        .Where(p => p.IdUsuario == idPersona &&
                                   p.TipoPersona == "Usuario" &&
                                   (p.EstadoProceso == "Abierto" || p.EstadoProceso == "EnCurso"))
                        .Include(p => p.TipoProceso)
                        .OrderByDescending(p => p.IdProceso)
                        .FirstOrDefaultAsync();
                }

                if (procesoActivo == null)
                {
                    Console.WriteLine("❌ No se encontró proceso activo (Abierto o EnCurso)");
                    return NotFound(new { message = "No hay proceso activo para este usuario" });
                }

                Console.WriteLine($"✅ Proceso activo encontrado: {procesoActivo.IdProceso} - Estado: {procesoActivo.EstadoProceso}");

                var registrosDelProceso = await _context.RegistroNFC
                    .Where(r => (tipoPersona == "Aprendiz" ? r.IdAprendiz == idPersona : r.IdUsuario == idPersona) &&
                                r.FechaRegistro >= procesoActivo.TimeStampEntradaSalida)
                    .ToListAsync();

                return Ok(new
                {
                    IdProceso = procesoActivo.IdProceso,
                    TipoPersona = procesoActivo.TipoPersona,
                    IdAprendiz = procesoActivo.IdAprendiz,
                    IdUsuario = procesoActivo.IdUsuario,
                    IdTipoProceso = procesoActivo.IdTipoProceso,
                    TimeStampEntradaSalida = procesoActivo.TimeStampEntradaSalida,
                    EstadoProceso = procesoActivo.EstadoProceso,
                    Observaciones = procesoActivo.Observaciones,
                    TieneIngreso = registrosDelProceso.Any(r => r.TipoRegistro == "Ingreso"),
                    TieneSalida = registrosDelProceso.Any(r => r.TipoRegistro == "Salida"),
                    CantidadRegistros = registrosDelProceso.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al buscar proceso activo.",
                    detalle = ex.Message
                });
            }
        }

        // ✅ PUT: api/Procesoes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProceso(int id, [FromBody] Proceso proceso)
        {
            ModelState.Remove(nameof(Proceso.Aprendiz));
            ModelState.Remove(nameof(Proceso.Usuario));
            ModelState.Remove(nameof(Proceso.TipoProceso));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Proceso.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Proceso no encontrado." });

            if (proceso.TipoPersona != "Usuario" && proceso.TipoPersona != "Aprendiz")
                return BadRequest(new { message = "TipoPersona debe ser 'Usuario' o 'Aprendiz'." });

            try
            {
                existing.IdAprendiz = proceso.IdAprendiz;
                existing.IdUsuario = proceso.IdUsuario;
                existing.IdTipoProceso = proceso.IdTipoProceso;
                existing.TipoPersona = proceso.TipoPersona;
                existing.RequiereOtrosProcesos = proceso.RequiereOtrosProcesos;
                existing.Observaciones = proceso.Observaciones;
                existing.SincronizadoBD = proceso.SincronizadoBD;
                existing.TimeStampEntradaSalida = proceso.TimeStampEntradaSalida ?? DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Proceso actualizado correctamente.",
                    proceso = existing
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al actualizar el proceso.",
                    detalle = ex.Message
                });
            }
        }

        // ✅ POST: api/Procesoes
        [HttpPost]
        public async Task<ActionResult<Proceso>> PostProceso([FromBody] Proceso proceso)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (proceso.TipoPersona != "Usuario" && proceso.TipoPersona != "Aprendiz")
                return BadRequest(new { message = "TipoPersona debe ser 'Usuario' o 'Aprendiz'." });

            proceso.TimeStampEntradaSalida ??= DateTime.Now;
            proceso.SincronizadoBD ??= false;
            proceso.RequiereOtrosProcesos ??= false;

            try
            {
                _context.Proceso.Add(proceso);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProceso), new { id = proceso.IdProceso }, proceso);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear el proceso.",
                    detalle = ex.Message
                });
            }
        }

        // ✅ DELETE: api/Procesoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProceso(int id)
        {
            var proceso = await _context.Proceso.FindAsync(id);
            if (proceso == null)
                return NotFound(new { message = "Proceso no encontrado." });

            try
            {
                _context.Proceso.Remove(proceso);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Proceso eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al eliminar el proceso.",
                    detalle = ex.Message
                });
            }
        }

        // ================================================
        // ENDPOINTS PARA GESTIÓN DE ESTADO DE PROCESO
        // ================================================

        /// <summary>
        /// Confirmar INGRESO - Cambiar estado de "Abierto" a "EnCurso"
        /// </summary>
        [HttpPost("confirmarIngreso/{id}")]
        public async Task<IActionResult> ConfirmarIngreso(int id)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n📥 ═══════════════════════════════════════");
                Console.WriteLine($"📥 CONFIRMANDO INGRESO");
                Console.WriteLine($"📥 Proceso ID: {id}");
                Console.WriteLine($"📥 ═══════════════════════════════════════");
                Console.ResetColor();

                var proceso = await _context.Proceso.FindAsync(id);
                if (proceso == null)
                {
                    Console.WriteLine($"❌ Proceso {id} no encontrado");
                    return NotFound(new { mensaje = "Proceso no encontrado" });
                }

                Console.WriteLine($"📋 Proceso encontrado:");
                Console.WriteLine($"   • ID: {proceso.IdProceso}");
                Console.WriteLine($"   • Estado actual: {proceso.EstadoProceso}");
                Console.WriteLine($"   • Tipo: {proceso.TipoPersona}");

                if (proceso.EstadoProceso != "Abierto")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ ERROR: Proceso no está Abierto (está {proceso.EstadoProceso})");
                    Console.ResetColor();
                    return BadRequest(new { mensaje = $"El proceso no está en estado Abierto (está {proceso.EstadoProceso})" });
                }

                // ✅ OBTENER DISPOSITIVOS VINCULADOS
                var dispositivosVinculados = await _context.ElementoProceso
                    .Where(ep => ep.IdProceso == id && !ep.QuedoEnSena)
                    .Include(ep => ep.Elemento)
                    .ToListAsync();

                Console.WriteLine($"📦 Dispositivos vinculados: {dispositivosVinculados.Count}");
                foreach (var ep in dispositivosVinculados)
                {
                    var serial = ep.Elemento?.Serial ?? "N/A";
                    Console.WriteLine($"   • {serial}");
                }

                // ✅ MARCAR TODOS LOS DISPOSITIVOS COMO VALIDADOS
                Console.WriteLine($"\n✅ Marcando dispositivos como validados...");
                foreach (var ep in dispositivosVinculados)
                {
                    ep.Validado = true;
                    _context.Entry(ep).State = EntityState.Modified;
                    Console.WriteLine($"   ✓ {ep.Elemento?.Serial ?? "N/A"} → Validado = true");
                }
                Console.WriteLine($"   ✅ {dispositivosVinculados.Count} dispositivos marcados como validados");

                // ✅ REGISTRAR INGRESO UNA SOLA VEZ
                Console.WriteLine($"\n💾 Creando registro de INGRESO en RegistroNFC...");

                var registroIngreso = new RegistroNFC
                {
                    IdAprendiz = proceso.TipoPersona == "Aprendiz" ? proceso.IdAprendiz : null,
                    IdUsuario = proceso.TipoPersona == "Usuario" ? proceso.IdUsuario : null,
                    TipoRegistro = "Ingreso",
                    FechaRegistro = DateTime.Now,
                    Estado = "Activo",
                    IdProceso = id  // ✨ NUEVO: Vincular con Proceso
                };

                _context.RegistroNFC.Add(registroIngreso);
                Console.WriteLine($"   ✅ Registro de ingreso creado");

                // ✅ CAMBIAR ESTADO A "EnCurso"
                Console.WriteLine($"\n🔄 Cambiando estado del proceso...");
                proceso.EstadoProceso = "EnCurso";
                Console.WriteLine($"   ✅ Estado cambiado: Abierto → EnCurso");

                // ✅ GUARDAR CAMBIOS (para obtener IdRegistro)
                Console.WriteLine($"\n💾 Guardando en base de datos...");
                await _context.SaveChangesAsync();
                Console.WriteLine($"   ✅ Cambios guardados - IdRegistro: {registroIngreso.IdRegistro}");

                // ✨ NUEVO: Crear detalles para cada dispositivo
                Console.WriteLine($"\n📝 Creando detalles de registro para cada dispositivo...");
                foreach (var ep in dispositivosVinculados)
                {
                    var detalle = new DetalleRegistroNFC
                    {
                        IdRegistroNFC = registroIngreso.IdRegistro,
                        IdElemento = ep.IdElemento,
                        IdProceso = id,
                        Accion = "Ingreso",
                        FechaHora = DateTime.Now,
                        Validado = ep.Validado
                    };

                    _context.DetalleRegistroNFC.Add(detalle);
                    var serial = ep.Elemento?.Serial ?? "N/A";
                    Console.WriteLine($"   ✓ Detalle creado: {serial} → Ingreso");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"   ✅ {dispositivosVinculados.Count} detalles guardados en BD");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ ═══════════════════════════════════════");
                Console.WriteLine($"✅ INGRESO CONFIRMADO EXITOSAMENTE");
                Console.WriteLine($"✅ Proceso: {id}");
                Console.WriteLine($"✅ Dispositivos registrados y validados: {dispositivosVinculados.Count}");
                Console.WriteLine($"✅ ═══════════════════════════════════════\n");
                Console.ResetColor();

                return Ok(new
                {
                    mensaje = "Ingreso confirmado exitosamente",
                    idProceso = proceso.IdProceso,
                    dispositivosRegistrados = dispositivosVinculados.Count,
                    estadoAnterior = "Abierto",
                    estadoNuevo = "EnCurso",
                    registroCreado = true
                });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ ═══════════════════════════════════════");
                Console.WriteLine($"❌ ERROR CRÍTICO en ConfirmarIngreso");
                Console.WriteLine($"❌ Mensaje: {ex.Message}");
                Console.WriteLine($"❌ StackTrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine($"❌ ═══════════════════════════════════════\n");
                Console.ResetColor();

                return StatusCode(500, new
                {
                    mensaje = "Error al confirmar ingreso",
                    detalle = ex.Message
                });
            }
        }

        /// <summary>
        /// Confirmar SALIDA - OPCIÓN A: Proceso reutilizado con QuedoEnSena
        /// </summary>
        [HttpPost("confirmarSalida/{id}")]
        public async Task<IActionResult> ConfirmarSalida(int id)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n🚪 ═══════════════════════════════════════");
                Console.WriteLine($"🚪 CONFIRMANDO SALIDA");
                Console.WriteLine($"🚪 Proceso ID: {id}");
                Console.WriteLine($"🚪 ═══════════════════════════════════════");
                Console.ResetColor();

                var proceso = await _context.Proceso.FindAsync(id);
                if (proceso == null)
                {
                    Console.WriteLine($"❌ Proceso {id} no encontrado");
                    return NotFound(new { mensaje = "Proceso no encontrado" });
                }

                Console.WriteLine($"📋 Proceso encontrado:");
                Console.WriteLine($"   • ID: {proceso.IdProceso}");
                Console.WriteLine($"   • Estado actual: {proceso.EstadoProceso}");

                if (proceso.EstadoProceso != "EnCurso" && proceso.EstadoProceso != "Abierto")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ ERROR: Proceso debe estar Abierto o EnCurso");
                    Console.ResetColor();
                    return BadRequest(new { mensaje = $"El proceso debe estar Abierto o EnCurso" });
                }

                // ✅ OBTENER DISPOSITIVOS DEL PROCESO ACTUAL
                var dispositivosProceso = await _context.ElementoProceso
                    .Where(ep => ep.IdProceso == id)
                    .Include(ep => ep.Elemento)
                    .ToListAsync();

                Console.WriteLine($"📦 Dispositivos en el proceso: {dispositivosProceso.Count}");

                // ✅ BUSCAR PENDIENTES DE PROCESOS ANTERIORES
                var idPropietario = proceso.TipoPersona == "Aprendiz" ? proceso.IdAprendiz : proceso.IdUsuario;
                var pendientesAnteriores = await _context.ElementoProceso
                    .Include(ep => ep.Elemento)
                    .Where(ep =>
                        ep.Elemento.IdPropietario == idPropietario &&
                        ep.Elemento.TipoPropietario == proceso.TipoPersona &&
                        ep.QuedoEnSena == true &&
                        ep.IdProceso != id) // De OTROS procesos
                    .ToListAsync();

                Console.WriteLine($"🔍 Pendientes de procesos anteriores: {pendientesAnteriores.Count}");

                int dispositivosQueSalen = 0;
                int dispositivosQueQuedan = 0;
                int pendientesLiberados = 0;

                // ✅ NUEVO: Guardar IDs de pendientes ANTES de moverlos
                var idsPendientesLiberados = new List<int>();

                // ✅ PASO 1: LIBERAR TODOS LOS PENDIENTES AUTOMÁTICAMENTE
                // Cuando se confirma salida, TODOS los pendientes se liberan
                // (El frontend los agrega visualmente pero no crea ElementoProceso)
                foreach (var pendiente in pendientesAnteriores)
                {
                    var serial = pendiente.Elemento?.Serial ?? "N/A";

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"   🔓 LIBERANDO pendiente automáticamente:");
                    Console.WriteLine($"      • Serial: {serial}");
                    Console.WriteLine($"      • IdElementoProceso: {pendiente.IdElementoProceso}");
                    Console.WriteLine($"      • Proceso anterior: {pendiente.IdProceso} → {id}");
                    Console.WriteLine($"      • QuedoEnSena: true → false");
                    Console.ResetColor();

                    // ✅ Guardar ID para skipear después
                    idsPendientesLiberados.Add(pendiente.IdElemento);

                    // ✅ Mover al proceso actual para liberar
                    pendiente.IdProceso = id;
                    pendiente.QuedoEnSena = false;
                    pendiente.Validado = true;
                    _context.Entry(pendiente).State = EntityState.Modified;

                    pendientesLiberados++;
                    dispositivosQueSalen++;

                    // ❌ ELIMINAR el registro duplicado si existe
                    var duplicado = dispositivosProceso.FirstOrDefault(ep => ep.IdElemento == pendiente.IdElemento);
                    if (duplicado != null && duplicado.IdElementoProceso != pendiente.IdElementoProceso)
                    {
                        Console.WriteLine($"      🗑️ Eliminando duplicado: IdElementoProceso {duplicado.IdElementoProceso}");
                        _context.ElementoProceso.Remove(duplicado);
                        dispositivosProceso.Remove(duplicado);
                    }
                }

                // ✅ PASO 2: PROCESAR DISPOSITIVOS NUEVOS (NO PENDIENTES)
                foreach (var ep in dispositivosProceso)
                {
                    var serial = ep.Elemento?.Serial ?? "N/A";

                    // ¿Ya fue procesado como pendiente liberado?
                    var yaLiberado = idsPendientesLiberados.Contains(ep.IdElemento);
                    if (yaLiberado) continue;

                    if (ep.QuedoEnSena)
                    {
                        dispositivosQueQuedan++;
                        ep.Validado = false;
                        _context.Entry(ep).State = EntityState.Modified;
                        Console.WriteLine($"   🟠 {serial} → QUEDA EN SENA");
                    }
                    else
                    {
                        dispositivosQueSalen++;
                        ep.Validado = true;
                        _context.Entry(ep).State = EntityState.Modified;
                        Console.WriteLine($"   ✅ {serial} → SALE");
                    }
                }

                Console.WriteLine($"\n📊 Resumen:");
                Console.WriteLine($"   • Salen: {dispositivosQueSalen}");
                Console.WriteLine($"   • Quedan: {dispositivosQueQuedan}");
                Console.WriteLine($"   • Pendientes liberados: {pendientesLiberados}");

                // ✅ REGISTRAR SALIDA
                var registroSalida = new RegistroNFC
                {
                    IdAprendiz = proceso.TipoPersona == "Aprendiz" ? proceso.IdAprendiz : null,
                    IdUsuario = proceso.TipoPersona == "Usuario" ? proceso.IdUsuario : null,
                    TipoRegistro = "Salida",
                    FechaRegistro = DateTime.Now,
                    Estado = "Activo",
                    IdProceso = id  // ✨ NUEVO: Vincular con Proceso
                };

                _context.RegistroNFC.Add(registroSalida);

                // ✅ CERRAR PROCESO
                proceso.EstadoProceso = "Cerrado";

                // ✅ GUARDAR para obtener IdRegistro
                await _context.SaveChangesAsync();
                Console.WriteLine($"   ✅ RegistroSalida creado - IdRegistro: {registroSalida.IdRegistro}");

                // ✨ NUEVO: Crear detalles para cada dispositivo
                Console.WriteLine($"\n📝 Creando detalles de salida...");
                var todosDispositivos = dispositivosProceso.Concat(pendientesAnteriores.Where(p => dispositivosProceso.Any(ep => ep.IdElemento == p.IdElemento))).ToList();
                
                foreach (var ep in dispositivosProceso)
                {
                    // Evitar duplicados de pendientes ya procesados
                    var yaLiberado = pendientesAnteriores.Any(p => p.IdElemento == ep.IdElemento);
                    if (yaLiberado) continue;

                    var accion = ep.QuedoEnSena ? "Quedo" : "Salida";
                    var detalle = new DetalleRegistroNFC
                    {
                        IdRegistroNFC = registroSalida.IdRegistro,
                        IdElemento = ep.IdElemento,
                        IdProceso = id,
                        Accion = accion,
                        FechaHora = DateTime.Now,
                        Validado = ep.Validado
                    };

                    _context.DetalleRegistroNFC.Add(detalle);
                    var serial = ep.Elemento?.Serial ?? "N/A";
                    Console.WriteLine($"   ✓ Detalle creado: {serial} → {accion}");
                }

                // ✅ Agregar detalles para TODOS los pendientes liberados
                foreach (var pendiente in pendientesAnteriores)
                {
                    var detalle = new DetalleRegistroNFC
                    {
                        IdRegistroNFC = registroSalida.IdRegistro,
                        IdElemento = pendiente.IdElemento,
                        IdProceso = id,
                        Accion = "Salida",  // Los pendientes liberados siempre salen
                        FechaHora = DateTime.Now,
                        Validado = true
                    };

                    _context.DetalleRegistroNFC.Add(detalle);
                    var serial = pendiente.Elemento?.Serial ?? "N/A";
                    Console.WriteLine($"   ✓ Detalle creado (pendiente liberado): {serial} → Salida");
                }

                await _context.SaveChangesAsync();
                var cantidadDetalles = dispositivosProceso.Count + pendientesLiberados;
                Console.WriteLine($"   ✅ {cantidadDetalles} detalles guardados en BD");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ SALIDA CONFIRMADA EXITOSAMENTE");
                if (pendientesLiberados > 0)
                {
                    Console.WriteLine($"✅ 🔓 {pendientesLiberados} pendiente(s) liberado(s)");
                }
                Console.WriteLine($"✅ ═══════════════════════════════════════\n");
                Console.ResetColor();

                return Ok(new
                {
                    mensaje = "Salida confirmada exitosamente",
                    idProceso = proceso.IdProceso,
                    dispositivosSalieron = dispositivosQueSalen,
                    dispositivosQuedaron = dispositivosQueQuedan,
                    pendientesLiberados = pendientesLiberados
                });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();

                return StatusCode(500, new { mensaje = "Error al confirmar salida", detalle = ex.Message });
            }
        }
        [HttpPost("marcarQuedoSena/{id}")]
        public async Task<IActionResult> MarcarQuedoEnSena(int id)
        {
            var ep = await _context.ElementoProceso.FindAsync(id);
            if (ep == null)
                return NotFound(new { message = "ElementoProceso no encontrado" });

            ep.QuedoEnSena = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Marcado como QuedoEnSena = false" });
        }


        // ✅ GET: api/Procesoes/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult> GetProcesosPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            const int maxPageSize = 100;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var totalCount = await _context.Proceso.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await _context.Proceso
                .Include(p => p.TipoProceso)
                .Include(p => p.Usuario)
                .Include(p => p.Aprendiz)
                .AsNoTracking()
                .OrderByDescending(p => p.TimeStampEntradaSalida)
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

        private bool ProcesoExists(int id)
        {
            return _context.Proceso.Any(e => e.IdProceso == id);
        }
    }

    // DTO para recibir datos (si lo necesitas)
    public class ProcesoConfirmacionDTO
    {
        public List<int>? IdsDispositivos { get; set; }
    }
}