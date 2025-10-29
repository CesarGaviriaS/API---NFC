using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API___NFC.Models
{
    public class TipoElemento
    {

        [Key]
        public int IdTipoElemento { get; set; }

        [Required, MaxLength(100)]
        public string Tipo { get; set; }

        public bool? RequiereNFC { get; set; }

        public bool? Estado { get; set; }

        public DateTime? FechaCreacion { get; set; }
    }
}