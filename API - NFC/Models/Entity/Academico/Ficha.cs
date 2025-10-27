using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Academico
{
    [Table("Ficha")]
    public class Ficha
    {
        [Key]
        public int IdFicha { get; set; }
        
        [Required]
        public int IdPrograma { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "date")]
        public DateTime FechaInicio { get; set; }
        
        [Required]
        [Column(TypeName = "date")]
        public DateTime FechaFinal { get; set; }
        
        public bool Estado { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
        
        [ForeignKey("IdPrograma")]
        public virtual Programa? Programa { get; set; }
    }
}