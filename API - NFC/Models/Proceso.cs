

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API___NFC.Models
{
    public class Proceso
    {
        [Key]
        public int IdProceso { get; set; }

        public int IdTipoProceso { get; set; }

        [Required, MaxLength(20)]
        public string TipoPersona { get; set; }

        public int IdGuardia { get; set; }

        public DateTime? TimeStampEntradaSalida { get; set; }

        public bool? RequiereOtrosProcesos { get; set; }

        public int? IdProceso_Relacionado { get; set; }

        public string Observaciones { get; set; } // text

        public bool? SincronizadoBD { get; set; }

        public int? IdAprendiz { get; set; }

        public int? IdUsuario { get; set; }

        // Navigation
        [ForeignKey("IdTipoProceso")]
        [JsonIgnore]
        public virtual TipoProceso TipoProceso { get; set; }

        [ForeignKey("IdAprendiz")]
        [JsonIgnore]
        public virtual Aprendiz Aprendiz { get; set; }

        [ForeignKey("IdUsuario")]
        [JsonIgnore]
        public virtual Usuario Usuario { get; set; }
    }
}