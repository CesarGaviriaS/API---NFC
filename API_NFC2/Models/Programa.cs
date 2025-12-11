using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API___NFC.Models
{
    public class Programa
    {
        [Key]
        public int IdPrograma { get; set; }

        [Required, MaxLength(200)]
        public string NombrePrograma { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Codigo { get; set; } = null!;

        [Required, MaxLength(30)]
        public string NivelFormacion { get; set; } = null!;

        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

       
        [JsonIgnore]
        public virtual ICollection<Ficha>? Fichas { get; set; } = new List<Ficha>();
    }
}