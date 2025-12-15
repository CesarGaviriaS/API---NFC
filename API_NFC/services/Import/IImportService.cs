using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API___NFC.Services.Import
{
    /// <summary>
    /// Servicio genérico para importación de CSV
    /// </summary>
    public interface IImportService
    {
        Task<ImportResult> ProcesarAsync(Stream fileStream, string separator = ";");
        Task<ImportResult> ProcesarAsync(Stream fileStream, Dictionary<string, List<string>> mapping, string separator = ";");
    }

    public interface IImportService<T> : IImportService
    {
    }

    public class ImportResult
    {
        public int TotalProcesados { get; set; }
        public int TotalExitosos { get; set; }
        public int TotalFallidos { get; set; }

        /// <summary>
        /// Errores y advertencias generados durante la importación
        /// </summary>
        public List<string> Errores { get; set; } = new List<string>();
    }
}