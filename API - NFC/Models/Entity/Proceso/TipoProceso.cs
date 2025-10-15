using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Proceso
{
    [Table("TipoProceso")]
    public class TipoProceso
    {
        [Key]
        public int IdTipoProceso { get; set; }
        public string? Tipo { get; set; }
        public bool Estado { get; set; } = true;
    }
}