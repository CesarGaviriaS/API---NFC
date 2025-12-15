using System;
using Microsoft.Extensions.DependencyInjection;

namespace API___NFC.Services.Import
{
    public class ImportServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ImportServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IImportService GetService(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("El nombre del tipo de entidad está vacío.");

            switch (entityType.Trim().ToLower())
            {
                case "programa":
                case "programas":
                    return _serviceProvider.GetRequiredService<ProgramaImportService>();

                case "ficha":
                case "fichas":
                    return _serviceProvider.GetRequiredService<FichaImportService>();

                case "aprendiz":
                case "aprendices":
                    return _serviceProvider.GetRequiredService<AprendizImportService>();

                case "funcionario":
                case "funcionarios":
                case "usuarios":
                    return _serviceProvider.GetRequiredService<UsuarioImportService>();

                default:
                    throw new ArgumentException($"Tipo de entidad '{entityType}' no soportado.");
            }
        }
    }
}