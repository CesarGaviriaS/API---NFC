
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API___NFC.Models
{
    public class RegistroNFC
    {
        [Key]
        public int IdRegistro { get; set; }

        public int IdAprendiz { get; set; }

        public int IdUsuario { get; set; }

        [Required, MaxLength(50)]
        public string TipoRegistro { get; set; }

        public DateTime? FechaRegistro { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; }

        // Navigation
        [ForeignKey("IdAprendiz")]
        [JsonIgnore]
        
        public virtual Aprendiz Aprendiz { get; set; }

        [ForeignKey("IdUsuario")]
        [JsonIgnore]
        public virtual Usuario Usuario { get; set; }
    }
}