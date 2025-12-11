

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace API___NFC.Models
{
    public class Ficha
    {
        [Key]
        public int IdFicha { get; set; }

        public int IdPrograma { get; set; }

        [Required, MaxLength(50)]
        public string Codigo { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFinal { get; set; }

        public bool? Estado { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

[ForeignKey("IdPrograma")]
    [JsonIgnore]
    [ValidateNever] 
    public virtual Programa Programa { get; set; }
}
}