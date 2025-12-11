
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API___NFC.Models
{

    public class Aprendiz
    {
        [Key]
        public int IdAprendiz { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = null!; // NOT NULL en BD

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = null!; // NOT NULL en BD

        [Required, MaxLength(5)]
        public string TipoDocumento { get; set; } = null!; // NOT NULL en BD

        [Required, MaxLength(20)]
        public string NumeroDocumento { get; set; } = null!; // NOT NULL en BD

        [Required, MaxLength(150)]
        public string Correo { get; set; } = null!; // NOT NULL en BD

        [MaxLength(100)]
        public string? CodigoBarras { get; set; } // NULLABLE en BD

        public int IdFicha { get; set; } // FK, NOT NULL en BD

        [MaxLength(20)]
        public string? Telefono { get; set; } // NULLABLE en BD -> usar string?

        [MaxLength(255)]
        public string? FotoUrl { get; set; } // NULLABLE en BD -> usar string?

        public bool? Estado { get; set; } // NULLABLE en BD

        public DateTime? FechaCreacion { get; set; } // NULLABLE en BD

        public DateTime? FechaActualizacion { get; set; } // NULLABLE en BD

        // Navegación (puede ser null si no se incluye)
        [ForeignKey("IdFicha")]
        [JsonIgnore] // opcional: evita serializar la relación por defecto
        public virtual Ficha? Ficha { get; set; }
    }
}
