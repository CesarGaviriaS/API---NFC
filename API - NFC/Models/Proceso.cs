using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API___NFC.Models
{
    public class Proceso
    {
        [Key]
        public int IdProceso { get; set; }

        public int IdTipoProceso { get; set; }

        [Required, MaxLength(20)]
        public string TipoPersona { get; set; } = null!;  

        public int IdGuardia { get; set; }  //  este campo no tiene FK definida en  la BD. 
                                            // En el futuro puede ser FK a Usuario, pero lo dejamos int por ahora.

        public DateTime? TimeStampEntradaSalida { get; set; }  

        public bool? RequiereOtrosProcesos { get; set; }  

        public int? IdProceso_Relacionado { get; set; }  

        public string? Observaciones { get; set; } 

        public bool? SincronizadoBD { get; set; }  

        public int? IdAprendiz { get; set; } 

        public int? IdUsuario { get; set; }  

        
        [ForeignKey(nameof(IdTipoProceso))]
        [JsonIgnore]
        public virtual TipoProceso? TipoProceso { get; set; }

        [ForeignKey(nameof(IdAprendiz))]
        [JsonIgnore]
        public virtual Aprendiz? Aprendiz { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        [JsonIgnore]
        public virtual Usuario? Usuario { get; set; }

        // (Opcional, si quieres navegar a su proceso relacionado)
        // [ForeignKey(nameof(IdProceso_Relacionado))]
        // [JsonIgnore]
        // public virtual Proceso? ProcesoRelacionado { get; set; }
    }
}
