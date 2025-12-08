using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API___NFC.Models
{
    /// <summary>
    /// Detalle de cada dispositivo en un registro de ingreso/salida.
    /// Permite trazabilidad completa de cada elemento.
    /// </summary>
    public class DetalleRegistroNFC
    {
        [Key]
        public int IdDetalleRegistro { get; set; }

        [Required]
        public int IdRegistroNFC { get; set; }

        [Required]
        public int IdElemento { get; set; }

        [Required]
        public int IdProceso { get; set; }

        /// <summary>
        /// Acción realizada: 'Ingreso', 'Salida', 'Quedo'
        /// </summary>
        [Required, MaxLength(20)]
        public string Accion { get; set; } = null!;

        public DateTime FechaHora { get; set; } = DateTime.Now;

        public bool? Validado { get; set; }

        // ===================================================
        // Navegaciones
        // ===================================================

        [ForeignKey(nameof(IdRegistroNFC))]
        [JsonIgnore]
        public virtual RegistroNFC? RegistroNFC { get; set; }

        [ForeignKey(nameof(IdElemento))]
        [JsonIgnore]
        public virtual Elemento? Elemento { get; set; }

        [ForeignKey(nameof(IdProceso))]
        [JsonIgnore]
        public virtual Proceso? Proceso { get; set; }
    }
}
