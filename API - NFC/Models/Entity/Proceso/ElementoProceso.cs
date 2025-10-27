using API___NFC.Models.Entity.Inventario;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Proceso
{
    [Table("ElementoProceso")]
    public class ElementoProceso
    {
        [Key]
        public int IdElementoProceso { get; set; }
        
        [Required]
        public int IdProceso { get; set; }
        
        [Required]
        public int IdElemento { get; set; }
        
        public bool Validado { get; set; } = false;

        [ForeignKey("IdProceso")]
        public virtual Proceso? Proceso { get; set; }

        [ForeignKey("IdElemento")]
        public virtual Elemento? Elemento { get; set; }
    }
}
