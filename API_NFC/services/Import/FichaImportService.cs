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

        protected override async Task<Ficha?> ParseRowAsync(
            string[] values, 
            Dictionary<string, int> headerMap, 
            int lineNumber, 
            ImportResult result)
        {
            // ================================
            // Cargar Programas en Cache
            // ================================
            if (_programasCache == null)
            {
                _programasCache = await _context.Programa
                    .ToDictionaryAsync(p => p.Codigo, p => p.IdPrograma);
            }

            // ================================
            // Obtener valores mapeados
            // ================================
            var codigoFicha = GetMappedValue(values, headerMap, "Codigo", new[] { "Codigo", "CodigoFicha" });
            var codigoPrograma = GetMappedValue(values, headerMap, "Codigo_Programa", new[] { "Codigo_Programa", "Programa" });
            var fechaInicioStr = GetMappedValue(values, headerMap, "FechaInicio", new[] { "FechaInicio" });
            var fechaFinStr = GetMappedValue(values, headerMap, "FechaFin", new[] { "FechaFin" });

            // Crear resumen de datos de la fila
            var rowData = $"[CodigoFicha: '{codigoFicha ?? "(vacío)"}', " +
                          $"CodigoPrograma: '{codigoPrograma ?? "(vacío)"}', " +
                          $"FechaInicio: '{fechaInicioStr ?? "(vacío)"}', " +
                          $"FechaFin: '{fechaFinStr ?? "(vacío)"}']";

            // ================================
            // Validar campos obligatorios
            // ================================
            var camposFaltantes = new List<string>();
            if (string.IsNullOrEmpty(codigoFicha)) camposFaltantes.Add("Codigo");
            if (string.IsNullOrEmpty(codigoPrograma)) camposFaltantes.Add("Codigo_Programa");

            if (camposFaltantes.Count > 0)
            {
                result.Errores.Add($"❌ Fila {lineNumber}: Faltan campos obligatorios ({string.Join(", ", camposFaltantes)}) → Datos: {rowData}");
                return null;
            }

            // ================================
            // Validar que el programa exista
            // ================================
            if (!_programasCache.ContainsKey(codigoPrograma))
            {
                result.Errores.Add($"❌ Fila {lineNumber}: El programa con código '{codigoPrograma}' no existe → Datos: {rowData}");
                return null;
            }

            // ================================
            // Validar duplicados en BD
            // ================================
            var duplicado = await _context.Ficha.AnyAsync(f => f.Codigo == codigoFicha);
            if (duplicado)
            {
                result.Errores.Add($"⚠️ Fila {lineNumber}: La ficha con código '{codigoFicha}' ya existe en la base de datos → Registro ignorado");
                return null;
            }

            // ================================
            // Manejo seguro de fechas
            // ================================
            if (!DateTime.TryParse(fechaInicioStr, out DateTime fechaInicio))
                fechaInicio = DateTime.Now;

            if (!DateTime.TryParse(fechaFinStr, out DateTime fechaFin))
                fechaFin = fechaInicio.AddMonths(6);

            // ================================
            // Construir objeto final
            // ================================
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