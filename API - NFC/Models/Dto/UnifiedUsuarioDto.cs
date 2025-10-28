namespace API___NFC.Models.Dto
{
    public class UnifiedUsuarioDto
    {
        public int IdUsuario { get; set; }
        public string Rol { get; set; }
        public AprendizDto? Aprendiz { get; set; }
        public FuncionarioDto? Funcionario { get; set; }
    }

    public class AprendizDto
    {
        public int IdAprendiz { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? Correo { get; set; }
        public string? CodigoBarras { get; set; }
        public string? Telefono { get; set; }
        public int IdFicha { get; set; }
        public FichaSimpleDto? Ficha { get; set; }
    }

    public class FuncionarioDto
    {
        public int IdFuncionario { get; set; }
        public string? Nombre { get; set; }
        public string? Documento { get; set; }
        public string? Detalle { get; set; }
    }

    public class FichaSimpleDto
    {
        public int IdFicha { get; set; }
        public string? Codigo { get; set; }
        public ProgramaSimpleDto? Programa { get; set; }
    }

    public class ProgramaSimpleDto
    {
        public string? NombrePrograma { get; set; }
    }
}
