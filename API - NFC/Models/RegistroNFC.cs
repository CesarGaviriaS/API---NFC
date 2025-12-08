using System;
using System.Collections.Generic;
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

        // ✨ NUEVO: Vinculación con Proceso
        public int? IdProceso { get; set; }

        // Relaciones
        [ForeignKey(nameof(IdAprendiz))]
        [JsonIgnore]
        public virtual Aprendiz? Aprendiz { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        [JsonIgnore]
        public virtual Usuario? Usuario { get; set; }

        // ✨ NUEVO: Navegación a Proceso
        [ForeignKey(nameof(IdProceso))]
        [JsonIgnore]
        public virtual Proceso? Proceso { get; set; }

        // ✨ NUEVO: Navegación inversa a DetalleRegistroNFC
        [JsonIgnore]
        public virtual ICollection<DetalleRegistroNFC>? Detalles { get; set; }
    }
}
