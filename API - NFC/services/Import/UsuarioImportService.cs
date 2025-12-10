using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_NFC.Data;
using API___NFC.Models;
using BCrypt.Net;

namespace API___NFC.Services.Import
{
    public class UsuarioImportService : BaseImportService<Usuario>
    {
        public UsuarioImportService(ApplicationDbContext context) : base(context) { }

        protected override async Task<Usuario?> ParseRowAsync(string[] values, Dictionary<string, int> headerMap, int lineNumber, ImportResult result)
        {
            var nombre = GetValue(values, headerMap, "Nombre");
            var apellido = GetValue(values, headerMap, "Apellido");
            var tipoDoc = GetValue(values, headerMap, "TipoDocumento");
            var numDoc = GetValue(values, headerMap, "NumeroDocumento");
            var correo = GetValue(values, headerMap, "Correo");
            var rol = GetValue(values, headerMap, "Rol");
            var pass = GetValue(values, headerMap, "Contraseña");

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) || 
                string.IsNullOrEmpty(tipoDoc) || string.IsNullOrEmpty(numDoc) || 
                string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(rol))
            {
                result.Errores.Add($"Fila {lineNumber}: Faltan campos obligatorios.");
                return null;
            }

            // Validar Rol
            var rolesValidos = new List<string> { "Administrador", "Guardia", "Funcionario" };
            if (!rolesValidos.Contains(rol))
            {
                result.Errores.Add($"Fila {lineNumber}: Rol '{rol}' inválido.");
                return null;
            }

            // Generar password si no viene
            if (string.IsNullOrEmpty(pass))
            {
                pass = numDoc; // Default: documento
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(pass);

            return new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                TipoDocumento = tipoDoc,
                NumeroDocumento = numDoc,
                Correo = correo,
                Rol = rol,
                Contraseña = passwordHash,
                Estado = true,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };
        }
    }
}
