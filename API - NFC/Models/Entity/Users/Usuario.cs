using API___NFC.Models.Entity.Inventario;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API___NFC.Models.Entity.Users
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        public int? IdFuncionario { get; set; }
        public int? IdAprendiz { get; set; }

       
        [ForeignKey("IdFuncionario")] 
        public virtual Funcionario? Funcionario { get; set; }

        [ForeignKey("IdAprendiz")]
        public virtual Aprendiz? Aprendiz { get; set; }

        public virtual ICollection<Elemento> Elementos { get; set; } = new List<Elemento>();
        public bool Estado { get; set; } = true;
    }
}