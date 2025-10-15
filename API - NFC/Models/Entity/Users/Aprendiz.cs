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
        public string? Nombre { get; set; }
        public string? Documento { get; set; }
        public int? IdFicha { get; set; }

        [ForeignKey("IdFicha")] // <-- ¡Ajuste!
        public virtual Ficha? Ficha { get; set; }
        public bool Estado { get; set; } = true;
    }
}