namespace API___NFC.Models.Dto
{
    // Este DTO ahora contiene todos los campos necesarios para el nuevo dashboard.
    public class ElementoDto
    {
        public int IdElemento { get; set; }
        public string? NombreElemento { get; set; }
        public string? Serial { get; set; }
        public string? Marca { get; set; }
        public string? CaracteristicasTecnicas { get; set; }
        public string? CaracteristicasFisicas { get; set; }

        public string? Detalles { get; set; }
        public string? Estado { get; set; } // Ej: "En Bodega", "Prestado"
        public string? ImageUrl { get; set; }
    }
}

