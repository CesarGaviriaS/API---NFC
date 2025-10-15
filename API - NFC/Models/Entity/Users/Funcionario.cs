using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Users
{
    [Table("Funcionario")]
    public class Funcionario
    {
        [Key]
        public int IdFuncionario { get; set; }
        public string? Nombre { get; set; }
        public string? Documento { get; set; }
        public string? Detalle { get; set; }
        public string? EsNatural { get; set; }
        public bool Estado { get; set; } = true;
    }
}