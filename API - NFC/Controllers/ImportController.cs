using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using API___NFC.Services.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API___NFC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class ImportController : ControllerBase
    {
        private readonly ImportServiceFactory _factory;

        public ImportController(ImportServiceFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Importa datos desde CSV con mapeo dinámico de columnas
        /// </summary>
        /// <param name="entityType">Tipo de entidad: programas, fichas, aprendices, funcionarios</param>
        /// <param name="file">Archivo CSV</param>
        /// <param name="columnMapping">JSON con mapeo: {"ColumnaDB": ["columnaCSV1", "columnaCSV2"]}</param>
        /// <param name="separator">Separador de columnas del CSV (por defecto ;)</param>
        [HttpPost("{entityType}")]
        public async Task<IActionResult> Importar(
            string entityType, 
            IFormFile file, 
            [FromForm] string? columnMapping = null,
            [FromForm] string separator = ";")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se ha proporcionado un archivo.");

            try
            {
                // Resolver el servicio adecuado
                IImportService service = _factory.GetService(entityType);

                // Parsear el mapeo de columnas si se proporcionó
                Dictionary<string, List<string>> mapping = new();
                
                if (!string.IsNullOrWhiteSpace(columnMapping))
                {
                    try
                    {
                        mapping = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
                            columnMapping,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        ) ?? new Dictionary<string, List<string>>();
                    }
                    catch (JsonException)
                    {
                        return BadRequest("El formato del mapeo de columnas es inválido.");
                    }
                }

                using (var stream = file.OpenReadStream())
                {
                    var result = await service.ProcesarAsync(stream, mapping, separator);
                    return Ok(result);
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}

