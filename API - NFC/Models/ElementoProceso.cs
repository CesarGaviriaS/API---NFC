
using System;

namespace API___NFC.Models
{
    public class ElementoProceso
    {
        public int IdElementoProceso { get; set; }
        public int IdProceso { get; set; }
        public int IdElemento { get; set; }
        public bool? Validado { get; set; }

        // Navegación
        public virtual Elemento Elemento { get; set; }
        public virtual Proceso Proceso { get; set; }
    }
}