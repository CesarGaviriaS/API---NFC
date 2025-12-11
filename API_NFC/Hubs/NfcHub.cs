using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading.Tasks;
using API_NFC.Data;
using API___NFC.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace API___NFC.Hubs
{
    public class NfcHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // 👮‍♂️ Guardia actualmente activo en la terminal
        private static int _currentGuardId = 0;

        // 🎯 Modo de operación del Hub
        private static string _modoOperacion = "TERMINAL"; // TERMINAL | GESTION_TAGS

        public NfcHub(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // --- REGISTRAR GUARDIA ACTIVO DESDE LA TERMINAL ---
        public async Task SetCurrentGuard(int guardId)
        {
            _currentGuardId = guardId;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"👮‍♂️ [NfcHub] Guardia activo actualizado: Id = {guardId}");
            Console.ResetColor();

            await Clients.All.SendAsync("GuardChanged", guardId);
        }

        // --- MODO MANUAL DESDE DASHBOARD (GESTIÓN DE TAGS) ---
        public async Task SetAgentModeToWrite(string data)
        {
            _modoOperacion = "GESTION_TAGS";
            Console.WriteLine($"🏷️ [NfcHub] Modo cambiado a: GESTION_TAGS (Escritura)");
            await Clients.All.SendAsync("RequestWriteMode", data);
        }

        public async Task SetAgentModeToClean()
        {
            _modoOperacion = "GESTION_TAGS";
            Console.WriteLine($"🧹 [NfcHub] Modo cambiado a: GESTION_TAGS (Limpieza)");
            await Clients.All.SendAsync("RequestCleanMode");
        }

        public async Task SetAgentModeToRead()
        {
            _modoOperacion = "GESTION_TAGS";
            Console.WriteLine($"📖 [NfcHub] Modo cambiado a: GESTION_TAGS (Lectura)");
            await Clients.All.SendAsync("RequestReadMode");
        }

        // --- CAMBIAR A MODO TERMINAL (llamado desde Terminal.cshtml) ---
        public async Task SetModoTerminal()
        {
            _modoOperacion = "TERMINAL";
            Console.WriteLine($"🖥️ [NfcHub] Modo cambiado a: TERMINAL");
            await Task.CompletedTask;
        }

        // --- AGENTE → API (LECTURA DE TAG) ---
        public async Task ProcesarLecturaTag(string tagData)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n📡 ═══════════════════════════════════════");
            Console.WriteLine($"📡 TAG RECIBIDO: {tagData}");
            Console.WriteLine($"📡 MODO ACTUAL: {_modoOperacion}");
            Console.WriteLine($"📡 ═══════════════════════════════════════");
            Console.ResetColor();

            // ✅ SIEMPRE ENVIAR A TODOS LOS CLIENTES
            await Clients.All.SendAsync("RecibirDatosTag", tagData);

            // 🎯 SI ESTÁ EN MODO GESTIÓN DE TAGS, NO PROCESAR AUTOMÁTICAMENTE
            if (_modoOperacion == "GESTION_TAGS")
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("🏷️ MODO GESTIÓN DE TAGS → No se procesa automáticamente");
                Console.WriteLine("   Esperando acción del usuario (Grabar/Limpiar/Leer)");
                Console.ResetColor();
                return;
            }

            // 🎯 MODO TERMINAL: ENVIAR A TERMINAL PARA PROCESAMIENTO MANUAL
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🖥️ MODO TERMINAL → Enviando al Terminal NFC");
            Console.ResetColor();

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var parts = tagData.Split(',');
                if (parts.Length != 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Formato inválido: no es tipo,id");
                    Console.ResetColor();

                    await Clients.All.SendAsync("OperacionError", "Formato de tag inválido");
                    return;
                }

                int tipo = int.Parse(parts[0]);
                int id = int.Parse(parts[1]);
                string tipoPersona = (tipo == 1) ? "Aprendiz" : "Usuario";

                Console.WriteLine($"   → TipoPersona: {tipoPersona}, ID: {id}");

                // 🔥 BUSCAR O CREAR PROCESO "ABIERTO" PARA EL TERMINAL
                Proceso procesoActivo = null;

                if (tipoPersona == "Aprendiz")
                {
                    procesoActivo = await context.Proceso
                        .Where(p => p.IdAprendiz == id &&
                                   p.TipoPersona == "Aprendiz" &&
                                   (p.EstadoProceso == "Abierto" || p.EstadoProceso == "EnCurso"))
                        .Include(p => p.TipoProceso)
                        .OrderByDescending(p => p.IdProceso)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    procesoActivo = await context.Proceso
                        .Where(p => p.IdUsuario == id &&
                                   p.TipoPersona == "Usuario" &&
                                   (p.EstadoProceso == "Abierto" || p.EstadoProceso == "EnCurso"))
                        .Include(p => p.TipoProceso)
                        .OrderByDescending(p => p.IdProceso)
                        .FirstOrDefaultAsync();
                }

                Proceso proceso;
                string nombreTipo;
                string estadoProceso;

                // 🔥 SI NO HAY PROCESO ACTIVO → CREAR PROCESO "ABIERTO" (INGRESO)
                if (procesoActivo == null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("🆕 No hay proceso activo → Creando proceso ABIERTO (Ingreso)");
                    Console.ResetColor();

                    nombreTipo = "Ingreso";

                    var tipoProc = await context.TipoProceso.FirstOrDefaultAsync(t => t.Tipo == nombreTipo);
                    if (tipoProc == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"❌ No existe TipoProceso '{nombreTipo}' en la base de datos");
                        Console.ResetColor();

                        await Clients.All.SendAsync("OperacionError", $"No existe TipoProceso '{nombreTipo}'");
                        return;
                    }

                    proceso = new Proceso
                    {
                        IdTipoProceso = tipoProc.IdTipoProceso,
                        TipoPersona = tipoPersona,
                        IdGuardia = _currentGuardId,
                        TimeStampEntradaSalida = DateTime.Now,
                        EstadoProceso = "Abierto", // ✅ MODO TERMINAL: Esperando confirmación
                        Observaciones = $"Proceso {nombreTipo} iniciado por NFC - Pendiente de confirmación",
                        SincronizadoBD = true,
                        IdAprendiz = (tipo == 1) ? id : null,
                        IdUsuario = (tipo == 2) ? id : null
                    };

                    context.Proceso.Add(proceso);
                    await context.SaveChangesAsync();

                    estadoProceso = "Abierto";

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✅ Proceso INGRESO creado:");
                    Console.WriteLine($"   • IdProceso: {proceso.IdProceso}");
                    Console.WriteLine($"   • Estado: Abierto (Pendiente de confirmación)");
                    Console.WriteLine($"   • Guardia: {_currentGuardId}");
                    Console.ResetColor();
                }
                // 🔥 SI HAY PROCESO ACTIVO → PREPARAR SALIDA
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"🔄 Proceso activo encontrado:");
                    Console.WriteLine($"   • IdProceso: {procesoActivo.IdProceso}");
                    Console.WriteLine($"   • Estado: {procesoActivo.EstadoProceso}");
                    Console.WriteLine($"   • Preparando SALIDA");
                    Console.ResetColor();

                    proceso = procesoActivo;
                    nombreTipo = "Salida";
                    estadoProceso = proceso.EstadoProceso;
                }

                // ✅ CREAR UN "REGISTRO TEMPORAL" SOLO PARA EL FRONTEND
                // (El registro real se creará cuando se confirme en el Terminal)
                var registroTemporal = new
                {
                    IdRegistro = 0,
                    TipoRegistro = nombreTipo,
                    FechaRegistro = DateTime.Now,
                    Estado = "Pendiente"
                };

                var result = new
                {
                    Message = $"✅ {nombreTipo} detectado. Esperando confirmación en Terminal.",
                    Proceso = new
                    {
                        proceso.IdProceso,
                        proceso.TipoPersona,
                        proceso.IdAprendiz,
                        proceso.IdUsuario,
                        proceso.TimeStampEntradaSalida,
                        EstadoProceso = estadoProceso,
                        proceso.Observaciones
                    },
                    Registro = registroTemporal
                };

                await Clients.All.SendAsync("OperacionProcesada", JsonSerializer.Serialize(result));

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("📤 Datos enviados al Terminal NFC");
                Console.WriteLine($"   • Usuario debe confirmar {nombreTipo} manualmente");
                Console.WriteLine($"═══════════════════════════════════════\n");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ ERROR en ProcesarLecturaTag:");
                Console.WriteLine($"   Mensaje: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                Console.ResetColor();

                await Clients.All.SendAsync("OperacionError", $"Error interno: {ex.Message}");
            }
        }


        // --- ESTADOS DEL AGENTE ---
        public async Task SendStatusUpdate(string message, string statusType)
            => await Clients.All.SendAsync("AgentStatusUpdate", message, statusType);

        public async Task SendOperationSuccess(string message, string verifiedData)
            => await Clients.All.SendAsync("OperationSuccess", message, verifiedData);

        public async Task SendOperationFailure(string errorMessage)
            => await Clients.All.SendAsync("OperationFailed", errorMessage);

        public async Task SendTagEvent(string tipoEvento, string mensaje)
            => await Clients.All.SendAsync(tipoEvento, mensaje);

        public async Task SendTagCleanedSuccess(string cleanedTagCode)
            => await Clients.All.SendAsync("TagCleanedSuccess", cleanedTagCode);
    }
}