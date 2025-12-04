using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API_NFC.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Services.Import
{
    public abstract class BaseImportService<T> : IImportService<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        
        // Almacena el mapeo dinámico para uso en ParseRowAsync
        protected Dictionary<string, List<string>> _columnMapping = new();

        public BaseImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResult> ProcesarAsync(Stream fileStream, Dictionary<string, List<string>> mapping, string separator = ";")
        {
            var result = new ImportResult();
            var entitiesToInsert = new List<T>();
            
            // Guardar el mapeo para uso en las clases derivadas
            _columnMapping = mapping ?? new Dictionary<string, List<string>>();

            // Procesar el separador (puede ser un caracter especial)
            char[] separatorChars = separator == "\\t" ? new[] { '\t' } : separator.ToCharArray();

            using (var reader = new StreamReader(fileStream))
            {
                // Skip header
                var headerLine = await reader.ReadLineAsync();
                if (headerLine == null) return result;

                var headers = headerLine.Split(separatorChars);
                var headerMap = new Dictionary<string, int>();
                for (int i = 0; i < headers.Length; i++)
                {
                    headerMap[headers[i].Trim().Replace("\r", "")] = i;
                }

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

            if (entitiesToInsert.Count > 0)
            {
                // Usar SqlBulkCopy
                var connectionString = _context.Database.GetConnectionString();
                using (var bulkCopy = new SqlBulkCopy(connectionString))
                {
                    // Obtener nombre de tabla
                    var entityType = _context.Model.FindEntityType(typeof(T));
                    var tableName = entityType?.GetTableName() ?? typeof(T).Name;
                    var schema = entityType?.GetSchema() ?? "dbo";

                    bulkCopy.DestinationTableName = $"[{schema}].[{tableName}]";
                    
                    // Mapeo automático de columnas basado en propiedades
                    var table = new DataTable();
                    var props = typeof(T).GetProperties();
                    
                    foreach (var prop in props)
                    {
                        // Ignorar propiedades de navegación (virtual) y [NotMapped]
                        if (prop.GetGetMethod().IsVirtual && !prop.PropertyType.IsValueType && prop.PropertyType != typeof(string)) continue;
                        if (Attribute.IsDefined(prop, typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute))) continue;

                        // Nullable types handling
                        var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        table.Columns.Add(prop.Name, type);
                        bulkCopy.ColumnMappings.Add(prop.Name, prop.Name);
                    }

                    foreach (var item in entitiesToInsert)
                    {
                        var row = table.NewRow();
                        foreach (var prop in props)
                        {
                            if (table.Columns.Contains(prop.Name))
                            {
                                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                            }
                        }
                        table.Rows.Add(row);
                    }

                    try
                    {
                        await bulkCopy.WriteToServerAsync(table);
                    }
                    catch (Exception ex)
                    {
                        // Parsear el mensaje de error para hacerlo más descriptivo
                        var errorMessage = ex.Message;
                        
                        if (errorMessage.Contains("duplicate key") || errorMessage.Contains("unique index"))
                        {
                            // Extraer el valor duplicado del mensaje
                            var duplicateValue = ExtractDuplicateKeyValue(errorMessage);
                            result.Errores.Add($"❌ Error de duplicados: Ya existen registros en la base de datos con valores únicos repetidos. Valor duplicado: {duplicateValue}");
                            result.Errores.Add("💡 Sugerencia: Revise que los códigos o identificadores únicos en su CSV no existan ya en la base de datos.");
                        }
                        else if (errorMessage.Contains("constraint"))
                        {
                            result.Errores.Add($"❌ Error de restricción de BD: {errorMessage}");
                            result.Errores.Add("💡 Sugerencia: Verifique que los datos cumplan con las restricciones de la base de datos.");
                        }
                        else
                        {
                            result.Errores.Add($"❌ Error crítico al guardar en BD: {errorMessage}");
                        }
                        
                        result.TotalExitosos = 0;
                        result.TotalFallidos = result.TotalProcesados;
                    }
                }
            }

            return result;
        }

        protected abstract Task<T?> ParseRowAsync(string[] values, Dictionary<string, int> headerMap, int lineNumber, ImportResult result);
        
        /// <summary>
        /// Obtiene el valor de una columna del CSV directamente por nombre
        /// </summary>
        protected string? GetValue(string[] values, Dictionary<string, int> headerMap, string columnName)
        {
            if (headerMap.ContainsKey(columnName) && headerMap[columnName] < values.Length)
            {
                return values[headerMap[columnName]].Trim();
            }
            return null;
        }

        /// <summary>
        /// Obtiene el valor mapeado para una columna de la BD.
        /// Si hay múltiples columnas CSV mapeadas, las concatena con un separador.
        /// </summary>
        /// <param name="values">Valores de la fila actual</param>
        /// <param name="headerMap">Mapa de encabezados del CSV</param>
        /// <param name="dbColumnName">Nombre de la columna en la BD</param>
        /// <param name="fallbackCsvColumns">Columnas CSV por defecto si no hay mapeo</param>
        /// <param name="separator">Separador para concatenación (por defecto espacio)</param>
        protected string? GetMappedValue(
            string[] values, 
            Dictionary<string, int> headerMap, 
            string dbColumnName,
            string[]? fallbackCsvColumns = null,
            string separator = " ")
        {
            // Buscar en el mapeo dinámico primero
            if (_columnMapping.TryGetValue(dbColumnName, out var csvColumns) && csvColumns.Count > 0)
            {
                var parts = csvColumns
                    .Select(col => GetValue(values, headerMap, col))
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();
                
                return parts.Count > 0 ? string.Join(separator, parts) : null;
            }

            // Fallback: usar las columnas por defecto si se proporcionaron
            if (fallbackCsvColumns != null && fallbackCsvColumns.Length > 0)
            {
                var parts = fallbackCsvColumns
                    .Select(col => GetValue(values, headerMap, col))
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();
                
                return parts.Count > 0 ? string.Join(separator, parts) : null;
            }

            // Fallback final: buscar el nombre de la columna directamente (compatibilidad)
            return GetValue(values, headerMap, dbColumnName);
        }

        /// <summary>
        /// Extrae el valor de clave duplicada del mensaje de error de SQL Server
        /// Ejemplo de mensaje: "The duplicate key value is (1001)."
        /// </summary>
        private static string ExtractDuplicateKeyValue(string errorMessage)
        {
            try
            {
                // Buscar el patrón "is (valor)"
                var startIndex = errorMessage.IndexOf("is (");
                if (startIndex >= 0)
                {
                    startIndex += 4; // Avanzar después de "is ("
                    var endIndex = errorMessage.IndexOf(")", startIndex);
                    if (endIndex > startIndex)
                    {
                        return errorMessage.Substring(startIndex, endIndex - startIndex);
                    }
                }
                
                // Buscar patrón alternativo "value is"
                startIndex = errorMessage.IndexOf("value is");
                if (startIndex >= 0)
                {
                    return errorMessage.Substring(startIndex);
                }
            }
            catch
            {
                // Si falla el parsing, retornar mensaje genérico
            }
            
            return "(No se pudo determinar el valor exacto)";
        }
    }
}

