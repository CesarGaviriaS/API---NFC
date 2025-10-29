

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API___NFC.Models
{
    public class ElementoProceso
    {
        [Key]
        public int IdElementoProceso { get; set; }

        public int IdProceso { get; set; }

        public int IdElemento { get; set; }

        public bool? Validado { get; set; }

        // Navigation
        [ForeignKey("IdProceso")]
        public virtual Proceso Proceso { get; set; }

        [ForeignKey("IdElemento")]
        [JsonIgnore]
        public virtual Elemento Elemento { get; set; }
    }
}