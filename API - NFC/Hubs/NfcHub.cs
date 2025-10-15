using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace API___NFC.Hubs
{
    public class NfcHub : Hub
    {
        // --- MÉTODO EXISTENTE (PARA EL DASHBOARD) ---
        public async Task TransmitirDatosTag(string tagData)
        {
            await Clients.All.SendAsync("RecibirDatosTag", tagData);
        }

        // --- NUEVOS MÉTODOS (CLIENTE WEB -> AGENTE) ---

        // Pide al agente que se prepare para escribir datos en el próximo tag
        public async Task SetAgentModeToWrite(string data)
        {
            await Clients.All.SendAsync("RequestWriteMode", data);
        }

        // Pide al agente que se prepare para limpiar el próximo tag
        public async Task SetAgentModeToClean()
        {
            await Clients.All.SendAsync("RequestCleanMode");
        }

        // Pide al agente que vuelva al modo de lectura continua (para el dashboard)
        public async Task SetAgentModeToRead()
        {
            await Clients.All.SendAsync("RequestReadMode");
        }


        // --- NUEVOS MÉTODOS (AGENTE -> CLIENTE WEB) ---

        // Envía actualizaciones de estado generales
        public async Task SendStatusUpdate(string message, string statusType)
        {
            await Clients.All.SendAsync("AgentStatusUpdate", message, statusType);
        }

        // Notifica que una operación (escritura/limpieza) fue exitosa
        public async Task SendOperationSuccess(string message, string verifiedData)
        {
            await Clients.All.SendAsync("OperationSuccess", message, verifiedData);
        }

        // Notifica que una operación falló
        public async Task SendOperationFailure(string errorMessage)
        {
            await Clients.All.SendAsync("OperationFailed", errorMessage);
        }
    }
}
