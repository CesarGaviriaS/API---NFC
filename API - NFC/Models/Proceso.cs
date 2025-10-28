
using System;
using System.Collections.Generic;

namespace API___NFC.Models
{
    public class Proceso
    {
        public int IdProceso { get; set; }
        public int IdTipoProceso { get; set; }
        public string TipoPersona { get; set; } // "Usuario" or "Aprendiz"
        public int IdGuardia { get; set; }
        public DateTime? TimeStampEntradaSalida { get; set; }
        public bool? RequiereOtrosProcesos { get; set; }
        public int? IdProceso_Relacionado { get; set; }
        public string Observaciones { get; set; }
        public bool? SincronizadoBD { get; set; }
        public int? IdAprendiz { get; set; }
        public int? IdUsuario { get; set; }

        // Navegación
        public virtual Aprendiz Aprendiz { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual TipoProceso TipoProceso { get; set; }
        public virtual ICollection<ElementoProceso> ElementoProcesos { get; set; } = new List<ElementoProceso>();
    }
}