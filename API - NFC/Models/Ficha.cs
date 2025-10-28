
using System;
using System.Collections.Generic;

namespace API___NFC.Models
{
    public class Ficha
    {
        public int IdFicha { get; set; }
        public int IdPrograma { get; set; }
        public string Codigo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinal { get; set; }
        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public virtual Programa Programa { get; set; }
        public virtual ICollection<Aprendiz> Aprendices { get; set; } = new List<Aprendiz>();
    }
}