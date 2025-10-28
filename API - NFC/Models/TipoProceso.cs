using System;
using System.Collections.Generic;

namespace API___NFC.Models
{
    public class TipoProceso
    {
        public int IdTipoProceso { get; set; }
        public string Tipo { get; set; }
        public bool? Estado { get; set; }

        // Navegación
        public virtual ICollection<Proceso> Procesos { get; set; } = new List<Proceso>();
    }
}