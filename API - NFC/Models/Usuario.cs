using System;
using System.ComponentModel.DataAnnotations;

namespace API___NFC.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required, MaxLength(100)]
        public string Apellido { get; set; }

        [Required, MaxLength(5)]
        public string TipoDocumento { get; set; }

        [Required, MaxLength(20)]
        public string NumeroDocumento { get; set; }

        [Required, MaxLength(150)]
        public string Correo { get; set; }

        [Required, MaxLength(255)]
        public string Contraseña { get; set; }

        [Required, MaxLength(20)]
        public string Rol { get; set; }

        [Required, MaxLength(100)]
        public string CodigoBarras { get; set; }

        [MaxLength(100)]
        public string? Cargo { get; set; } 

        [MaxLength(20)]
        public string? Telefono { get; set; } 

        [MaxLength(255)]
        public string? FotoUrl { get; set; } 
        public bool? Estado { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }
    }
}