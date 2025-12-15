using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_NFC.Data;
using API___NFC.Models;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Services.Import
{
    public class AprendizImportService : BaseImportService<Aprendiz>
    {
        private Dictionary<string, int>? _fichasCache;

        public AprendizImportService(ApplicationDbContext context) : base(context) { }

        protected override async Task<Aprendiz?> ParseRowAsync(
            string[] values,
            Dictionary<string, int> headerMap,
            int lineNumber,
            ImportResult result)
        {
            // =============================
            // Cargar cache de fichas
            // =============================
            if (_fichasCache == null)
            {
                _fichasCache = await _context.Ficha
                    .ToDictionaryAsync(f => f.Codigo, f => f.IdFicha);
            }

            // =============================
            // Obtener valores del CSV
            // =============================
            var nombre = GetMappedValue(values, headerMap, "Nombre", new[] { "Nombre" });
            var apellido = GetMappedValue(values, headerMap, "Apellido", new[] { "Apellido" });
            var tipoDoc = GetMappedValue(values, headerMap, "TipoDocumento", new[] { "TipoDocumento", "Tipo_Documento" });
            var numDoc = GetMappedValue(values, headerMap, "NumeroDocumento", new[] { "NumeroDocumento", "NumDocumento" });
            var correo = GetMappedValue(values, headerMap, "Correo", new[] { "Correo", "Email" });
            var numFicha = GetMappedValue(values, headerMap, "Numero_Ficha", new[] { "Numero_Ficha", "Ficha" });
            var codigoBarras = GetMappedValue(values, headerMap, "CodigoBarras", new[] { "CodigoBarras", "Barras" });

            // Resumen de datos de la fila para errores
            var rowData = $"[Nombre: '{nombre ?? "(vacío)"}', Apellido: '{apellido ?? "(vacío)"}', " +
                          $"TipoDocumento: '{tipoDoc ?? "(vacío)"}', NumDoc: '{numDoc ?? "(vacío)"}', " +
                          $"Correo: '{correo ?? "(vacío)"}', Ficha: '{numFicha ?? "(vacío)"}']";

            // =============================
            // Validar campos obligatorios
            // =============================
            var faltantes = new List<string>();
            if (string.IsNullOrEmpty(nombre)) faltantes.Add("Nombre");
            if (string.IsNullOrEmpty(apellido)) faltantes.Add("Apellido");
            if (string.IsNullOrEmpty(tipoDoc)) faltantes.Add("TipoDocumento");
            if (string.IsNullOrEmpty(numDoc)) faltantes.Add("NumeroDocumento");
            if (string.IsNullOrEmpty(correo)) faltantes.Add("Correo");
            if (string.IsNullOrEmpty(numFicha)) faltantes.Add("Numero_Ficha");

            if (faltantes.Count > 0)
            {
                result.Errores.Add($"❌ Fila {lineNumber}: Faltan campos obligatorios ({string.Join(", ", faltantes)}) → Datos: {rowData}");
                return null;
            }

            // =============================
            // Validar ficha existente
            // =============================
            if (!_fichasCache.ContainsKey(numFicha))
            {
                result.Errores.Add($"❌ Fila {lineNumber}: La ficha '{numFicha}' no existe → Datos: {rowData}");
                return null;
            }

            // =============================
            // Validar duplicado en BD
            // =============================
            var existeDoc = await _context.Aprendiz
                .AnyAsync(a => a.NumeroDocumento == numDoc);

            if (existeDoc)
            {
                result.Errores.Add($"⚠️ Fila {lineNumber}: El aprendiz con documento '{numDoc}' ya existe → Ignorado");
                return null;
            }

            // =============================
            // Normalizar TipoDocumento
            // =============================
            tipoDoc = tipoDoc.Replace(".", "").Trim().ToUpper();

            // =============================
            // Generar Código de Barras si falta
            // =============================
            if (string.IsNullOrEmpty(codigoBarras))
            {
                codigoBarras = numDoc;
            }

            // =============================
            // Construir objeto final
            // =============================
            return new Aprendiz
            {
                Nombre = nombre,
                Apellido = apellido,
                TipoDocumento = tipoDoc,
                NumeroDocumento = numDoc,
                Correo = correo,
                CodigoBarras = codigoBarras,
                IdFicha = _fichasCache[numFicha],
                Estado = true,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };
        }
    }
}