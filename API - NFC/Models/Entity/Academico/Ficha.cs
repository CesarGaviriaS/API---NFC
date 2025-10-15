using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Academico // o tu namespace
{
    [Table("Ficha")]
    public class Ficha
    {
        [Key]
        public int IdFicha { get; set; }
        public string? Codigo { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinal { get; set; }
        public int? IdPrograma { get; set; }
        [ForeignKey("IdPrograma")] 
        public virtual Programa? Programa { get; set; }
        public bool Estado { get; set; } = true;
    }
}