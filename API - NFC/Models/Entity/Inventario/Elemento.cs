using API___NFC.Models.Entity.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Inventario // o tu namespace
{
    [Table("Elemento")]
    public class Elemento
    {
        [Key]
        public int IdElemento { get; set; }

        public int? IdTipoElemento { get; set; }

        [Column("elemento")]
        public string? NombreElemento { get; set; }

        public string? Serial { get; set; }
        public string? CaracteristicasTecnicas { get; set; }
        public string? CaracteristicasFisicas { get; set; }
        public string? Detalles { get; set; }

        public int? IdPropietario { get; set; }

        public string? Marca { get; set; }
        public bool? TieneNFCTag { get; set; }

        [Column("imageUrl")]
        public string? ImageUrl { get; set; }

        [ForeignKey("IdPropietario")] 
        public virtual Usuario? Propietario { get; set; }

        [ForeignKey("IdTipoElemento")]
        public virtual TipoElemento? TipoElemento { get; set; }
        public bool Estado { get; set; } = true;
    }
}