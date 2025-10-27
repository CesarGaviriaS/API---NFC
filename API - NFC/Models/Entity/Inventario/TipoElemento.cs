using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Inventario
{
    [Table("TipoElemento")]
    public class TipoElemento
    {
        [Key]
        public int IdTipoElemento { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Tipo")]
        public string Tipo { get; set; } = string.Empty;
        
        public bool RequiereNFC { get; set; } = false;
        
        public bool Estado { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}