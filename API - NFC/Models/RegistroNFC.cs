using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API___NFC.Models
{
    public class RegistroNFC
    {
        [Key]
        public int IdRegistro { get; set; }
        
        public int? IdAprendiz { get; set; } = null;


        public int? IdUsuario { get; set; } = null;

        [Required, MaxLength(50)]
        public string TipoRegistro { get; set; } = null!; // Lectura | Escritura | Limpieza | etc.

        public DateTime? FechaRegistro { get; set; } // DB ya tiene GETDATE()

        [MaxLength(20)]
        public string? Estado { get; set; } // p.ej. "Activo" | "Inactivo"

        // Relaciones
        [ForeignKey(nameof(IdAprendiz))]
        [JsonIgnore]
        public virtual Aprendiz? Aprendiz { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        [JsonIgnore]
        public virtual Usuario? Usuario { get; set; }
    }
}
