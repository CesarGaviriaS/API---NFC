using API___NFC.Models.Entity.Users;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ClaveUsuario
{
    [Key]
    public int IdClave { get; set; }

    [Required]
    public int IdUsuario { get; set; }

    [ForeignKey("IdUsuario")]
    public virtual Usuario Usuario { get; set; }

    [Required]
    public string ContrasenaHash { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
