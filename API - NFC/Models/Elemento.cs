

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API___NFC.Models
{
    public class Elemento
    {
        [Key]
        public int IdElemento { get; set; }

        public int IdTipoElemento { get; set; }

        public int IdPropietario { get; set; }

        [Required, MaxLength(20)]
        public string TipoPropietario { get; set; }

        [MaxLength(100)]
        public string Marca { get; set; }

        [MaxLength(100)]
        public string Modelo { get; set; }

        [Required, MaxLength(150)]
        public string Serial { get; set; }

        [MaxLength(100)]
        public string CodigoNFC { get; set; }

        public string Descripcion { get; set; } // text

        [MaxLength(255)]
        public string ImagenUrl { get; set; }

        public bool? Estado { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        // Navigation
        [ForeignKey("IdTipoElemento")]
        public virtual TipoElemento TipoElemento { get; set; }
    }
}