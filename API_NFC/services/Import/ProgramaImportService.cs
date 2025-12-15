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

        protected override async Task<Programa?> ParseRowAsync(
            string[] values,
            Dictionary<string, int> headerMap,
            int lineNumber,
            ImportResult result)
        {
            // Obtener valores usando el mapeo dinámico
            var codigo = GetMappedValue(values, headerMap, "Codigo", new[] { "Codigo" });
            var nombre = GetMappedValue(values, headerMap, "NombrePrograma", new[] { "NombrePrograma" });
            var nivel = GetMappedValue(values, headerMap, "NivelFormacion", new[] { "NivelFormacion" });

            // Construcción de la info de la fila (para mensajes claros)
            var rowData =
                $"[Codigo: '{codigo ?? "(vacío)"}', NombrePrograma: '{nombre ?? "(vacío)"}', NivelFormacion: '{nivel ?? "(vacío)"}']";

            // VALIDACIONES
            var camposFaltantes = new List<string>();

            if (string.IsNullOrWhiteSpace(codigo)) camposFaltantes.Add("Codigo");
            if (string.IsNullOrWhiteSpace(nombre)) camposFaltantes.Add("NombrePrograma");
            if (string.IsNullOrWhiteSpace(nivel)) camposFaltantes.Add("NivelFormacion");

            if (camposFaltantes.Count > 0)
            {
                result.Errores.Add(
                    $"❌ Fila {lineNumber}: Faltan campos obligatorios ({string.Join(", ", camposFaltantes)}) → Datos: {rowData}"
                );
                return null;
            }

            // Verificar si ya existe (mismo código)
            bool existeEnBd = await _context.Programa.AnyAsync(p => p.Codigo == codigo);

            if (existeEnBd)
            {
                result.Errores.Add(
                    $"⚠️ Fila {lineNumber}: El código '{codigo}' ya existe → registro duplicado ignorado"
                );
                return null;
            }

            // Validación opcional nivel
            var nivelesValidos = new List<string>
            {
                "Tecnólogo", "Técnico", "Operario", "Auxiliar", "Especialización"
            };

            if (!nivelesValidos.Contains(nivel))
            {
                result.Errores.Add(
                    $"⚠️ Fila {lineNumber}: NivelFormacion '{nivel}' no es estándar (se aceptará igualmente)"
                );
            }

            // Crear y devolver entidad válida
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