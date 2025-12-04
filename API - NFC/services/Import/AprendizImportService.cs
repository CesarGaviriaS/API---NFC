using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override async Task<Aprendiz?> ParseRowAsync(string[] values, Dictionary<string, int> headerMap, int lineNumber, ImportResult result)
        {
            if (_fichasCache == null)
            {
                _fichasCache = await _context.Ficha
                    .ToDictionaryAsync(f => f.Codigo, f => f.IdFicha);
            }

            var nombre = GetValue(values, headerMap, "Nombre");
            var apellido = GetValue(values, headerMap, "Apellido");
            var tipoDoc = GetValue(values, headerMap, "TipoDocumento");
            var numDoc = GetValue(values, headerMap, "NumeroDocumento");
            var correo = GetValue(values, headerMap, "Correo");
            var numFicha = GetValue(values, headerMap, "Numero_Ficha");
            var codigoBarras = GetValue(values, headerMap, "CodigoBarras"); // Opcional en CSV, obligatorio en BD

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) || 
                string.IsNullOrEmpty(tipoDoc) || string.IsNullOrEmpty(numDoc) || 
                string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(numFicha))
            {
                result.Errores.Add($"Fila {lineNumber}: Faltan campos obligatorios.");
                return null;
            }

            if (!_fichasCache.ContainsKey(numFicha))
            {
                result.Errores.Add($"Fila {lineNumber}: La ficha '{numFicha}' no existe.");
                return null;
            }

            // Normalizar TipoDocumento
            tipoDoc = tipoDoc.Replace(".", "").ToUpper();

            // Default CodigoBarras si no viene
            if (string.IsNullOrEmpty(codigoBarras))
            {
                codigoBarras = numDoc;
            }

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
