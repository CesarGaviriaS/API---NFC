using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Inventario
{
    [Table("TipoElemento")]
    public class TipoElemento
    {
        [Key]
        public int IdTipoElemento { get; set; }

        [Column("TipoElemento")] // Especifica el nombre exacto de la columna
        public string? NombreTipoElemento { get; set; }
        public bool Estado { get; set; } = true;
    }
}