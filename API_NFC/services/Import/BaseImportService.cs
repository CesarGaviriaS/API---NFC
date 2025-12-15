using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API_NFC.Data;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Services.Import
{
    public abstract class BaseImportService<T> : IImportService<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        // Mapeo dinámico de columnas
        protected Dictionary<string, List<string>> _columnMapping = new();

        public BaseImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================================================
        //  MÉTODO REQUERIDO POR LA INTERFAZ (OBLIGATORIO)
        // ===============================================================
        public Task<ImportResult> ProcesarAsync(Stream fileStream, string separator = ";")
        {
            // Si no envían mapping, enviamos uno vacío
            return ProcesarAsync(fileStream, new Dictionary<string, List<string>>(), separator);
        }

        // ===============================================================
        //  MÉTODO PRINCIPAL DE IMPORTACIÓN (CON MAPPING)
        // ===============================================================
        public async Task<ImportResult> ProcesarAsync(
            Stream fileStream,
            Dictionary<string, List<string>> mapping,
            string separator = ";")
        {
            var result = new ImportResult();
            var entitiesToInsert = new List<T>();

            _columnMapping = mapping ?? new Dictionary<string, List<string>>();

            char[] separatorChars = separator == "\\t" ? new[] { '\t' } : separator.ToCharArray();

            using (var reader = new StreamReader(fileStream))
            {
                var headerLine = await reader.ReadLineAsync();
                if (headerLine == null) return result;

                var headers = headerLine.Split(separatorChars);
                var headerMap = new Dictionary<string, int>();

                for (int i = 0; i < headers.Length; i++)
                    headerMap[headers[i].Trim().Replace("\r", "")] = i;

                int lineNumber = 1;

                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(separatorChars);

                    try
                    {
                        var entity = await ParseRowAsync(values, headerMap, lineNumber, result);
                        if (entity != null)
                        {
                            entitiesToInsert.Add(entity);
                            result.TotalExitosos++;
                        }
                        else
                        {
                            result.TotalFallidos++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errores.Add($"Fila {lineNumber}: Error inesperado - {ex.Message}");
                        result.TotalFallidos++;
                    }

                    result.TotalProcesados++;
                }
            }

            // ===============================================================
            //  GUARDAR EN BASE DE DATOS (POSTGRES NORMAL)
            // ===============================================================
            try
            {
                if (entitiesToInsert.Count > 0)
                {
                    await _context.AddRangeAsync(entitiesToInsert);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result.Errores.Add($"❌ Error al guardar en PostgreSQL: {ex.Message}");
                result.TotalFallidos = result.TotalProcesados;
                result.TotalExitosos = 0;
            }

            return result;
        }

        // ===============================================================
        //  MÉTODO ABSTRACTO QUE CADA SERVICIO DEBE IMPLEMENTAR
        // ===============================================================
        protected abstract Task<T?> ParseRowAsync(
            string[] values,
            Dictionary<string, int> headerMap,
            int lineNumber,
            ImportResult result);

        // ===============================================================
        //  OBTENER VALOR DE UNA COLUMNA DEL CSV
        // ===============================================================
        protected string? GetValue(string[] values, Dictionary<string, int> headerMap, string columnName)
        {
            if (headerMap.ContainsKey(columnName) && headerMap[columnName] < values.Length)
                return values[headerMap[columnName]].Trim();

            return null;
        }

        // ===============================================================
        //  MAPEADO DINÁMICO (OPCIONAL)
        // ===============================================================
        protected string? GetMappedValue(
            string[] values,
            Dictionary<string, int> headerMap,
            string dbColumnName,
            string[]? fallbackCsvColumns = null,
            string separator = " ")
        {
            if (_columnMapping.TryGetValue(dbColumnName, out var csvColumns) && csvColumns.Count > 0)
            {
                var parts = csvColumns
                    .Select(col => GetValue(values, headerMap, col))
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();

                return parts.Count > 0 ? string.Join(separator, parts) : null;
            }

            if (fallbackCsvColumns != null)
            {
                var parts = fallbackCsvColumns
                    .Select(col => GetValue(values, headerMap, col))
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();

                return parts.Count > 0 ? string.Join(separator, parts) : null;
            }

            return GetValue(values, headerMap, dbColumnName);
        }
    }
}