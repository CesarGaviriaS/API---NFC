using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Academico
{
    [Table("Programa")]
    public class Programa
    {
        [Key]
        public int IdPrograma { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string NombrePrograma { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(30)]
        public string NivelFormacion { get; set; } = string.Empty;
        
        public bool Estado { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}