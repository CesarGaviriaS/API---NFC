using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API___NFC.Models
{
    public class TipoProceso
    {
        [Key]
        public int IdTipoProceso { get; set; }

        [Required, MaxLength(50)]
        public string Tipo { get; set; }

        public bool? Estado { get; set; }
    }
}