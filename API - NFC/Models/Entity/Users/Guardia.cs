// Models/Guardia.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models
{
    [Table("Guardia")]
    public class Guardia
    {
        [Key]
        public int IdGuardia { get; set; }
        public string? Nombre { get; set; }
        public string? Documento { get; set; }
        public bool Estado { get; set; } = true;
    }
}