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
            return entityType.ToLower() switch
            {
                "programas" => _serviceProvider.GetRequiredService<ProgramaImportService>(),
                "fichas" => _serviceProvider.GetRequiredService<FichaImportService>(),
                "aprendices" => _serviceProvider.GetRequiredService<AprendizImportService>(),
                "funcionarios" => _serviceProvider.GetRequiredService<UsuarioImportService>(),
                _ => throw new ArgumentException("Tipo de entidad no soportado")
            };
        }
    }
}
