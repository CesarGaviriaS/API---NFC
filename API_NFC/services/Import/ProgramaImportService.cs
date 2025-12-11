using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_NFC.Data;
using API___NFC.Models;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Services.Import
{
    public class ProgramaImportService : BaseImportService<Programa>
    {
        public ProgramaImportService(ApplicationDbContext context) : base(context) { }

        protected override async Task<Programa?> ParseRowAsync(string[] values, Dictionary<string, int> headerMap, int lineNumber, ImportResult result)
        {
            // Obtener valores
            var codigo = GetMappedValue(values, headerMap, "Codigo", new[] { "Codigo" });
            var nombre = GetMappedValue(values, headerMap, "NombrePrograma", new[] { "NombrePrograma" });
            var nivel = GetMappedValue(values, headerMap, "NivelFormacion", new[] { "NivelFormacion" });

            // Crear resumen de datos de la fila para mensajes de error
            var rowData = $"[Codigo: '{codigo ?? "(vacío)"}', NombrePrograma: '{nombre ?? "(vacío)"}', NivelFormacion: '{nivel ?? "(vacío)"}']";

            // Validar campos obligatorios
            var camposFaltantes = new List<string>();
            if (string.IsNullOrEmpty(codigo)) camposFaltantes.Add("Codigo");
            if (string.IsNullOrEmpty(nombre)) camposFaltantes.Add("NombrePrograma");
            if (string.IsNullOrEmpty(nivel)) camposFaltantes.Add("NivelFormacion");

            if (camposFaltantes.Count > 0)
            {
                result.Errores.Add($"❌ Fila {lineNumber}: Faltan campos obligatorios ({string.Join(", ", camposFaltantes)}) → Datos: {rowData}");
                return null;
            }

            // Verificar si ya existe en BD (para mensajes más claros de duplicados)
            var existeEnBd = await _context.Set<Programa>().AnyAsync(p => p.Codigo == codigo);
            if (existeEnBd)
            {
                result.Errores.Add($"⚠️ Fila {lineNumber}: El código '{codigo}' ya existe en la base de datos → Registro duplicado ignorado");
                return null;
            }

            // Validar NivelFormacion (opcional)
            var nivelesValidos = new List<string> { "Tecnólogo", "Técnico", "Operario", "Auxiliar", "Especialización" };
            if (!nivelesValidos.Contains(nivel))
            {
                // Solo advertencia, no falla
                result.Errores.Add($"⚠️ Fila {lineNumber}: Nivel de formación '{nivel}' no es estándar (se aceptará igualmente)");
            }

            return new Programa
            {
                Codigo = codigo,
                NombrePrograma = nombre,
                NivelFormacion = nivel,
                Estado = true,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };
        }
    }
}

