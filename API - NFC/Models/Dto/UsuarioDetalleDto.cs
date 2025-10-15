using API___NFC.Models.Entity.Inventario;
using System.Xml.Linq;

namespace API___NFC.Models.Dto
{
    public class UsuarioDetalleDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Documento { get; set; }
        public string Rol { get; set; } // "Aprendiz" o "Funcionario"
        public List<Elemento> Elementos { get; set; } = new List<Elemento>();
        public bool Estado { get; set; } = true;
    }
}
