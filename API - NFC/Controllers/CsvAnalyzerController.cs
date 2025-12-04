using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using API___NFC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class CsvAnalyzerController : ControllerBase
    {
        /// <summary>
        /// Analiza un archivo CSV y retorna los encabezados de las columnas
        /// </summary>
        [HttpPost("headers")]
        public async Task<IActionResult> GetCsvHeaders(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se ha proporcionado un archivo.");

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var headerLine = await reader.ReadLineAsync();
                
                if (string.IsNullOrWhiteSpace(headerLine))
                    return BadRequest("El archivo está vacío o no tiene encabezados.");

                var headers = headerLine.Split(';')
                    .Select(h => h.Trim())
                    .Where(h => !string.IsNullOrEmpty(h))
                    .ToList();

                return Ok(headers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al leer el archivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Retorna el esquema de columnas para una entidad específica
        /// </summary>
        [HttpGet("schema/{entityType}")]
        public IActionResult GetTableSchema(string entityType)
        {
            var schema = entityType.ToLower() switch
            {
                "programas" => GetSchemaFromType<Programa>(),
                "fichas" => GetSchemaFromType<Ficha>(),
                "aprendices" => GetSchemaFromType<Aprendiz>(),
                "funcionarios" => GetSchemaFromType<Usuario>(),
                _ => null
            };

            if (schema == null)
                return BadRequest($"Tipo de entidad '{entityType}' no soportado.");

            return Ok(schema);
        }

        /// <summary>
        /// Extrae el esquema de columnas de un tipo de modelo
        /// </summary>
        private List<ColumnSchema> GetSchemaFromType<T>()
        {
            var schema = new List<ColumnSchema>();
            var properties = typeof(T).GetProperties();

            // Propiedades que ignorar (claves, navegación, auto-generadas)
            var ignoreProps = new HashSet<string> 
            { 
                "IdAprendiz", "IdUsuario", "IdFicha", "IdPrograma",
                "FechaCreacion", "FechaActualizacion", "Estado",
                "Ficha", "Programa", "Fichas", // Navegación
                "TokenRecuperacion", "FechaTokenExpira" // Campos internos
            };

            foreach (var prop in properties)
            {
                // Ignorar propiedades de navegación y campos internos
                if (ignoreProps.Contains(prop.Name))
                    continue;

                // Ignorar propiedades virtuales (navegación)
                if (prop.GetGetMethod()?.IsVirtual == true && 
                    !prop.PropertyType.IsValueType && 
                    prop.PropertyType != typeof(string))
                    continue;

                var isRequired = prop.GetCustomAttribute<RequiredAttribute>() != null;
                var maxLength = prop.GetCustomAttribute<MaxLengthAttribute>()?.Length;

                schema.Add(new ColumnSchema
                {
                    Name = prop.Name,
                    Required = isRequired,
                    MaxLength = maxLength,
                    Type = GetFriendlyTypeName(prop.PropertyType)
                });
            }

            return schema;
        }

        private string GetFriendlyTypeName(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            
            return underlyingType.Name switch
            {
                "String" => "texto",
                "Int32" => "número",
                "DateTime" => "fecha",
                "Boolean" => "booleano",
                _ => underlyingType.Name.ToLower()
            };
        }
    }

    public class ColumnSchema
    {
        public string Name { get; set; } = string.Empty;
        public bool Required { get; set; }
        public int? MaxLength { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
