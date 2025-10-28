using System;
using System.Collections.Generic;

namespace API___NFC.Models
{
    public class Programa
    {
        public int IdPrograma { get; set; }
        public string NombrePrograma { get; set; }
        public string Codigo { get; set; }
        public string NivelFormacion { get; set; } // Operario, Especialización, Tecnólogo, Técnico
        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public virtual ICollection<Ficha> Fichas { get; set; } = new List<Ficha>();
    }
}