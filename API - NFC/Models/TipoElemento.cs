using System;
using System.Collections.Generic;

namespace API___NFC.Models
{
    public class TipoElemento
    {
        public int IdTipoElemento { get; set; }
        public string Tipo { get; set; }
        public bool? RequiereNFC { get; set; }
        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }

        // Navegación
        public virtual ICollection<Elemento> Elementos { get; set; } = new List<Elemento>();
    }
}