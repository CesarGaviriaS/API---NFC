using API___NFC.Models.Entity.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Proceso
{
    [Table("Proceso")]
    public class Proceso
    {
        [Key]
        public int IdProceso { get; set; }
        
        [Required]
        public int IdTipoProceso { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string TipoPersona { get; set; } = string.Empty;
        
        [Required]
        public int IdGuardia { get; set; }
        
        public DateTime TimeStampEntradaSalida { get; set; } = DateTime.Now;
        
        public bool RequiereOtrosProcesos { get; set; } = false;
        
        public int? IdProceso_Relacionado { get; set; }
        
        [Column(TypeName = "text")]
        public string? Observaciones { get; set; }
        
        public bool SincronizadoBD { get; set; } = false;
        
        public int? IdAprendiz { get; set; }
        
        public int? IdUsuario { get; set; }

        [ForeignKey("IdTipoProceso")]
        public virtual TipoProceso? TipoProceso { get; set; }
        
        [ForeignKey("IdAprendiz")]
        public virtual Aprendiz? Aprendiz { get; set; }
        
        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}