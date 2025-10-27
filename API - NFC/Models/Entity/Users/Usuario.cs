using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Users
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(5)]
        public string TipoDocumento { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(150)]
        public string Correo { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Contraseña { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Rol { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string CodigoBarras { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Cargo { get; set; }
        
        [MaxLength(20)]
        public string? Telefono { get; set; }
        
        [MaxLength(255)]
        public string? FotoUrl { get; set; }
        
        public bool Estado { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}