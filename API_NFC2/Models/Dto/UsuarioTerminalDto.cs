using System.Collections.Generic;

namespace API___NFC.Models.Dto
{
    // Este DTO ahora contiene el perfil completo del usuario.
    public class UsuarioTerminalDto
    {
        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Documento { get; set; }
        public string? Programa { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Rh { get; set; }
        public string? Ficha { get; set; }

        // Nuevos campos para la tarjeta de perfil
        public string? Rol { get; set; } // "Aprendiz" o "Funcionario"
        public string? NivelFormacion { get; set; } // "Tecnólogo", "Técnico", etc.
        public List<ElementoDto> Elementos { get; set; }
        public bool Estado { get; set; } = true;
    }
}
