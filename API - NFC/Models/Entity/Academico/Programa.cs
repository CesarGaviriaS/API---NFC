using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Academico
{
    [Table("Programa")]
    public class Programa
    {
        [Key]
        public int IdPrograma { get; set; }
        public string? NombrePrograma { get; set; }
        public string? Codigo { get; set; }
        public string? NivelFormacion { get; set; }
        public bool Estado { get; set; } = true;
    }
}