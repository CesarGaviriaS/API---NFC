using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace API___NFC.Models
{
    public class ElementoProceso
    {
        [Key]
        public int IdElementoProceso { get; set; }

        [Required]
        public int IdElemento { get; set; }

        [Required]
        public int IdProceso { get; set; }

        public bool? Validado { get; set; }

        //  Navegaciones opcionales (evitan el error 400)
        [ForeignKey("IdElemento")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Elemento? Elemento { get; set; }

        [ForeignKey("IdProceso")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Proceso? Proceso { get; set; }
    }
}
