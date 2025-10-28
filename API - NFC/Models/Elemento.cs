
using System;

namespace API___NFC.Models
{
    public class Elemento
    {
        public int IdElemento { get; set; }
        public int IdTipoElemento { get; set; }
        public int IdPropietario { get; set; }
        public string TipoPropietario { get; set; } // "Usuario" or "Aprendiz"
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Serial { get; set; }
        public string CodigoNFC { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public virtual TipoElemento TipoElemento { get; set; }
    }
}