// Models/FlujoNfcItemDto.cs
namespace API___NFC.Models
{
    public class FlujoNfcItemDto
    {
        public int IdRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string TipoRegistro { get; set; } = string.Empty;
        public string TipoPersona { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string DispositivosTexto { get; set; } = string.Empty;
    }
}