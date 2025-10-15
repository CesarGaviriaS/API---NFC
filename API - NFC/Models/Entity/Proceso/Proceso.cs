using API___NFC.Models.Entity.Inventario;
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
        public int? IdTipoProceso { get; set; }
        public string? TimeStampActual { get; set; }
        public int? IdElemento { get; set; }
        public string? RequiereOtroProceso { get; set; }
        public int? IdPortador { get; set; }
        [ForeignKey("IdPortador")]
        public virtual Usuario? Portador { get; set; }

        [ForeignKey("IdElemento")]
        public virtual Elemento? Elemento { get; set; }

        [ForeignKey("IdTipoProceso")]
        public virtual TipoProceso? TipoProceso { get; set; }
        public bool Estado { get; set; } = true;
    }
}