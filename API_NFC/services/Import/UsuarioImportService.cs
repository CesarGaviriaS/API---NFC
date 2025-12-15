using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_NFC.Data;
using API___NFC.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace API___NFC.Services.Import
{
    public class UsuarioImportService : BaseImportService<Usuario>
    {
        public UsuarioImportService(ApplicationDbContext context) : base(context) { }

        protected override async Task<Usuario?> ParseRowAsync(string[] values, Dictionary<string, int> headerMap, int lineNumber, ImportResult result)
        {
            // Obtener datos (soporta mapeo dinámico)
            var nombre = GetMappedValue(values, headerMap, "Nombre", new[] { "Nombre" });
            var apellido = GetMappedValue(values, headerMap, "Apellido", new[] { "Apellido" });
            var tipoDoc = GetMappedValue(values, headerMap, "TipoDocumento", new[] { "TipoDocumento" });
            var numDoc = GetMappedValue(values, headerMap, "NumeroDocumento", new[] { "NumeroDocumento" });
            var correo = GetMappedValue(values, headerMap, "Correo", new[] { "Correo" });
            var rol = GetMappedValue(values, headerMap, "Rol", new[] { "Rol" });
            var pass = GetMappedValue(values, headerMap, "Contraseña", new[] { "Contraseña" });

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(tipoDoc) ||
                string.IsNullOrWhiteSpace(numDoc) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(rol))
            {
                result.Errores.Add($"❌ Fila {lineNumber}: Faltan campos obligatorios.");
                return null;
            }

            // Normalizar texto
            tipoDoc = tipoDoc.Trim().ToUpper();
            rol = rol.Trim();

            // Validar roles permitidos
            var rolesValidos = new List<string> { "Administrador", "Guardia", "Funcionario" };

            if (!rolesValidos.Contains(rol))
            {
                result.Errores.Add($"❌ Fila {lineNumber}: Rol '{rol}' inválido. Roles válidos: Administrador, Guardia, Funcionario");
                return null;
            }

            // Si no trae contraseña → usar documento como default
            if (string.IsNullOrEmpty(pass))
                pass = numDoc;

            // Hash de contraseña
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(pass);

            // Verificar duplicados en DB (solo advertencia, no detiene importación)
            var existe = await _context.Usuario.AnyAsync(u => u.NumeroDocumento == numDoc);
            if (existe)
            {
                result.Errores.Add($"⚠️ Fila {lineNumber}: Usuario con documento '{numDoc}' ya existe → Se ignora");
                return null;
            }

            // Crear usuario
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