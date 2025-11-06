using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading.Tasks;
using API_NFC.Data;
using API___NFC.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace API___NFC.Hubs
{
    public class NfcHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public NfcHub(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // --- MODO MANUAL DESDE DASHBOARD ---
        public async Task SetAgentModeToWrite(string data)
            => await Clients.All.SendAsync("RequestWriteMode", data);

        public async Task SetAgentModeToClean()
            => await Clients.All.SendAsync("RequestCleanMode");

        public async Task SetAgentModeToRead()
            => await Clients.All.SendAsync("RequestReadMode");

        // --- AGENTE → API (LECTURA DE TAG) ---
        public async Task ProcesarLecturaTag(string tagData)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"📡 Procesando tag recibido: {tagData}");
            Console.ResetColor();

            await Clients.All.SendAsync("RecibirDatosTag", tagData);

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

                Console.WriteLine($"→ TipoPersona: {tipoPersona}, ID: {id}");

                // Buscar el tipo de proceso alternando ingreso/salida
                var ultimo = await context.RegistroNFC
                    .Where(r => (tipo == 1 ? r.IdAprendiz == id : r.IdUsuario == id))
                    .OrderByDescending(r => r.FechaRegistro)
                    .FirstOrDefaultAsync();

                string nombreTipo = (ultimo == null || ultimo.TipoRegistro == "Salida") ? "Ingreso" : "Salida";
                Console.WriteLine($"→ Tipo de proceso detectado: {nombreTipo}");

                var tipoProc = await context.TipoProceso.FirstOrDefaultAsync(t => t.Tipo == nombreTipo);
                if (tipoProc == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ No existe TipoProceso '{nombreTipo}' en la base de datos");
                    Console.ResetColor();

                    await Clients.All.SendAsync("OperacionError", $"No existe TipoProceso '{nombreTipo}'");
                    return;
                }

                var proceso = new Proceso
                {
                    IdTipoProceso = tipoProc.IdTipoProceso,
                    TipoPersona = tipoPersona,
                    IdGuardia = 6, // Simulación del guardia
                    TimeStampEntradaSalida = DateTime.Now,
                    Observaciones = $"{nombreTipo} registrada por NFC",
                    SincronizadoBD = true,
                    IdAprendiz = (tipo == 1) ? id : null,
                    IdUsuario = (tipo == 2) ? id : null
                };

                context.Proceso.Add(proceso);
                await context.SaveChangesAsync();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Proceso creado correctamente. IdProceso = {proceso.IdProceso}");
                Console.ResetColor();

                var registro = new RegistroNFC
                {
                    IdAprendiz = (tipo == 1) ? id : null,
                    IdUsuario = (tipo == 2) ? id : null,
                    TipoRegistro = nombreTipo,
                    FechaRegistro = DateTime.Now,
                    Estado = "Activo"
                };

                context.RegistroNFC.Add(registro);
                await context.SaveChangesAsync();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Registro NFC guardado correctamente. IdRegistro = {registro.IdRegistro}");
                Console.ResetColor();

                var result = new
                {
                    Message = "✅ Proceso y Registro creados correctamente.",
                    Proceso = proceso,
                    Registro = registro
                };

                await Clients.All.SendAsync("OperacionProcesada", JsonSerializer.Serialize(result));

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("📤 Notificación enviada a clientes conectados.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error interno en ProcesarLecturaTag: {ex.Message}");
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
        // --- NUEVO EVENTO GLOBAL PARA MENSAJES EN TIEMPO REAL ---
        public async Task SendTagEvent(string tipoEvento, string mensaje)
        {
            await Clients.All.SendAsync(tipoEvento, mensaje);
        }

    }
}
