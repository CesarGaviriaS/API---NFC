﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace API___NFC.Models
{
    public class Elemento
    {
        [Key]
        public int IdElemento { get; set; }

        public int IdTipoElemento { get; set; }

        public int IdPropietario { get; set; }

        [Required, MaxLength(20)]
        public string TipoPropietario { get; set; } = null!;

        [MaxLength(100)]
        public string? Marca { get; set; }  // ✅ Nullable

        [MaxLength(100)]
        public string? Modelo { get; set; }  // ✅ Nullable

        [Required, MaxLength(150)]
        public string Serial { get; set; } = null!;

        [MaxLength(100)]
        public string? CodigoNFC { get; set; }  // ✅ Nullable

        public string? Descripcion { get; set; }  // ✅ Nullable

        [MaxLength(255)]
        public string? ImagenUrl { get; set; }  // ✅ Nullable

        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Navigation
        [ForeignKey("IdTipoElemento")]
        [JsonIgnore]
        [ValidateNever]  // ✅ Añade esto
        public virtual TipoElemento? TipoElemento { get; set; }  // ✅ Nullable
    }
}