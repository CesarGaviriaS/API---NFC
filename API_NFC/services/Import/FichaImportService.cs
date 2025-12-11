using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_NFC.Data;
using API___NFC.Models;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Services.Import
{
    public class FichaImportService : BaseImportService<Ficha>
    {
        private Dictionary<string, int>? _programasCache;

        public FichaImportService(ApplicationDbContext context) : base(context) { }

        protected override async Task<Ficha?> ParseRowAsync(string[] values, Dictionary<string, int> headerMap, int lineNumber, ImportResult result)
        {
            // Cargar cache si es la primera vez
            if (_programasCache == null)
            {
                _programasCache = await _context.Programa
                    .ToDictionaryAsync(p => p.Codigo, p => p.IdPrograma);
            }

            var codigoFicha = GetValue(values, headerMap, "Codigo");
            var codigoPrograma = GetValue(values, headerMap, "Codigo_Programa");
            var fechaInicioStr = GetValue(values, headerMap, "FechaInicio");
            var fechaFinStr = GetValue(values, headerMap, "FechaFin");

            if (string.IsNullOrEmpty(codigoFicha) || string.IsNullOrEmpty(codigoPrograma))
            {
                result.Errores.Add($"Fila {lineNumber}: Faltan campos obligatorios (Codigo, Codigo_Programa)");
                return null;
            }

            if (!_programasCache.ContainsKey(codigoPrograma))
            {
                result.Errores.Add($"Fila {lineNumber}: El programa con código '{codigoPrograma}' no existe.");
                return null;
            }

            if (!DateTime.TryParse(fechaInicioStr, out DateTime fechaInicio)) fechaInicio = DateTime.Now;
            if (!DateTime.TryParse(fechaFinStr, out DateTime fechaFin)) fechaFin = DateTime.Now.AddMonths(6);

            return new Ficha
            {
                Codigo = codigoFicha,
                IdPrograma = _programasCache[codigoPrograma],
                FechaInicio = fechaInicio,
                FechaFinal = fechaFin,
                Estado = true,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };
        }
    }
}
