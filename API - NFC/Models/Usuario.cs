using System;

namespace API___NFC.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string Correo { get; set; }
        // property name changed to avoid accented char in C#
        public string Contrasena { get; set; }
        public string Rol { get; set; }
        public string CodigoBarras { get; set; }
        public string Cargo { get; set; }
        public string Telefono { get; set; }
        public string FotoUrl { get; set; }
        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}