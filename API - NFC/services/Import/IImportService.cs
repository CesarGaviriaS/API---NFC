using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API___NFC.Services.Import
{
    public interface IImportService
    {
        /// <summary>
        /// Procesa un archivo CSV con mapeo dinámico de columnas
        /// </summary>
        /// <param name="fileStream">Stream del archivo CSV</param>
        /// <param name="mapping">Mapeo de columnas: {"ColumnaDB": ["csvCol1", "csvCol2"]}</param>
        /// <param name="separator">Separador de columnas del CSV</param>
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
        public List<string> Errores { get; set; } = new List<string>();
    }
}
