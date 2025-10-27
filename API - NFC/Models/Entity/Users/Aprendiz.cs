using API___NFC.Models.Entity.Academico;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Users
{
    [Table("Aprendiz")]
    public class Aprendiz
    {
        [Key]
        public int IdAprendiz { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(5)]
        public string TipoDocumento { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(150)]
        public string Correo { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string CodigoBarras { get; set; } = string.Empty;
        
        [Required]
        public int IdFicha { get; set; }
        
        [MaxLength(20)]
        public string? Telefono { get; set; }
        
        [MaxLength(255)]
        public string? FotoUrl { get; set; }
        
        public bool Estado { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        [ForeignKey("IdFicha")]
        public virtual Ficha? Ficha { get; set; }
    }
}