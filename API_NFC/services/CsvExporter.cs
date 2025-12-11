using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using API___NFC.Models;

namespace API___NFC.Services.ExportHelpers
{
    /// <summary>
    /// Exportador CSV para reportes de flujo NFC.
    /// Genera archivos CSV con formato compatible con Excel y UTF-8 con BOM.
    /// </summary>
    public static class CsvExporter
    {
        /// <summary>
        /// Genera un archivo CSV a partir de los datos de flujo NFC.
        /// </summary>
        /// <param name="datos">Colección de registros de flujo NFC</param>
        /// <returns>Bytes del archivo CSV en formato UTF-8 con BOM</returns>
        public static byte[] GenerateCsv(IEnumerable<FlujoNfcItemDto> datos)
        {
            var sb = new StringBuilder();

            // Encabezados de columnas
            sb.AppendLine("Fecha / Hora,Tipo,Persona,Documento,Dispositivos,Tipo Persona");

            // Filas de datos
            foreach (var row in datos)
            {
                if (row == null) continue;

                string fecha = row.FechaRegistro == DateTime.MinValue
                    ? "N/A"
                    : row.FechaRegistro.ToString("yyyy-MM-dd HH:mm");

                string tipoRegistro = EscapeCsvField(row.TipoRegistro ?? string.Empty);
                string nombreCompleto = EscapeCsvField(row.NombreCompleto ?? string.Empty);
                string documento = EscapeCsvField(row.Documento ?? string.Empty);
                string dispositivos = EscapeCsvField(row.DispositivosTexto ?? string.Empty);
                string tipoPersona = EscapeCsvField(row.TipoPersona ?? string.Empty);

                sb.AppendLine($"{fecha},{tipoRegistro},{nombreCompleto},{documento},{dispositivos},{tipoPersona}");
            }

            // UTF-8 con BOM para mejor compatibilidad con Excel
            var encoding = new UTF8Encoding(true);
            return encoding.GetBytes(sb.ToString());
        }

        /// <summary>
        /// Escapa un campo CSV según el estándar RFC 4180.
        /// Si el campo contiene coma, comillas o salto de línea, lo envuelve en comillas
        /// y duplica las comillas internas.
        /// </summary>
        /// <param name="field">Campo a escapar</param>
        /// <returns>Campo escapado correctamente</returns>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return field;

            // Si contiene coma, comillas o salto de línea, debe ir entre comillas
            bool needsQuotes = field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r');

            if (!needsQuotes)
                return field;

            // Duplicar comillas internas y envolver en comillas
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
    }
}
