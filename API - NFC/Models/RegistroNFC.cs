using System;

namespace API___NFC.Models
{
    public class RegistroNFC
    {
        public int IdRegistro { get; set; }
        public int IdAprendiz { get; set; }
        public int IdUsuario { get; set; }
        public string TipoRegistro { get; set; } // nvarchar(50)
        public DateTime? FechaRegistro { get; set; }
        public string Estado { get; set; } // nvarchar(20)

        // Navegación
        public virtual Aprendiz Aprendiz { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}