
using System;

namespace API___NFC.Models
{
    public class Aprendiz
    {
        public int IdAprendiz { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string TipoDocumento { get; set; } // PA|CE|TI|CC
        public string NumeroDocumento { get; set; }
        public string Correo { get; set; }
        public string CodigoBarras { get; set; }
        public int IdFicha { get; set; }
        public string Telefono { get; set; }
        public string FotoUrl { get; set; }
        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public virtual Ficha Ficha { get; set; }
    }
}