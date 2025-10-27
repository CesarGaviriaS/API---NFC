using API___NFC.Models.Entity.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity
{
    [Table("RegistroNFC")]
    public class RegistroNFC
    {
        [Key]
        public int IdRegistro { get; set; }
        
        [Required]
        public int IdAprendiz { get; set; }
        
        [Required]
        public int IdUsuario { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string TipoRegistro { get; set; } = string.Empty;
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        [MaxLength(20)]
        public string? Estado { get; set; }

        [ForeignKey("IdAprendiz")]
        public virtual Aprendiz? Aprendiz { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}
