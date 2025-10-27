using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Inventario
{
    [Table("Elemento")]
    public class Elemento
    {
        [Key]
        public int IdElemento { get; set; }

        [Required]
        public int IdTipoElemento { get; set; }
        
        [Required]
        public int IdPropietario { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string TipoPropietario { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Marca { get; set; }
        
        [MaxLength(100)]
        public string? Modelo { get; set; }
        
        [Required]
        [MaxLength(150)]
        public string Serial { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? CodigoNFC { get; set; }
        
        [Column(TypeName = "text")]
        public string? Descripcion { get; set; }
        
        [MaxLength(255)]
        public string? ImagenUrl { get; set; }
        
        public bool Estado { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        [ForeignKey("IdTipoElemento")]
        public virtual TipoElemento? TipoElemento { get; set; }
    }
}