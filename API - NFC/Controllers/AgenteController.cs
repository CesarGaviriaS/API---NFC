using API___NFC.Hubs; // Asegúrate que el namespace de tu Hub sea correcto
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace API___NFC.Controllers
{
    public class TareaEscrituraRequest
    {
        public string Datos { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AgenteController : ControllerBase
    {
        private readonly IHubContext<NfcHub> _hubContext;

        public AgenteController(IHubContext<NfcHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("preparar-escritura")]
        public async Task<IActionResult> PrepararEscritura([FromBody] TareaEscrituraRequest request)
        {
            if (string.IsNullOrEmpty(request.Datos))
            {
                return BadRequest("Los datos para escribir no pueden estar vacíos.");
            }

            // Invoca el método "RecibirTareaEscritura" en los clientes SignalR (tu agente C#)
            await _hubContext.Clients.All.SendAsync("RecibirTareaEscritura", request.Datos);

            return Ok(new { message = "Comando de escritura enviado al agente." });
        }

        [HttpPost("preparar-limpieza")]
        public async Task<IActionResult> PrepararLimpieza()
        {
            // Invoca el método "RecibirTareaLimpieza" en los clientes SignalR
            await _hubContext.Clients.All.SendAsync("RecibirTareaLimpieza");
            return Ok(new { message = "Comando de limpieza enviado al agente." });
        }
    }
}