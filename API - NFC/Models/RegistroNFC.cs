using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models
{
    public class RegistroNFC
    {
        [Key]
        public int IdRegistro { get; set; }

        public int IdAprendiz { get; set; }

       
        public int IdUsuario { get; set; }

        [Required, MaxLength(50)]
        public string TipoRegistro { get; set; } = null!; // Lectura | Escritura | Limpieza | etc.

        public DateTime? FechaRegistro { get; set; } // DB ya tiene GETDATE()

        [MaxLength(20)]
        public string? Estado { get; set; } // p.ej. "Activo" | "Inactivo"

        // Relaciones
        [ForeignKey(nameof(IdAprendiz))]
        public virtual Aprendiz? Aprendiz { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public virtual Usuario? Usuario { get; set; }
    }
}
