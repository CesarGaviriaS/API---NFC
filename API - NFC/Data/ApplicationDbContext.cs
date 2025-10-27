using API___NFC.Models;
using API___NFC.Models.Entity;
using API___NFC.Models.Entity.Academico;
using API___NFC.Models.Entity.Inventario;
using API___NFC.Models.Entity.Proceso;
using API___NFC.Models.Entity.Users;
using Microsoft.EntityFrameworkCore;

namespace API___NFC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}
        
        public DbSet<Aprendiz> Aprendices { get; set; }
        public DbSet<Elemento> Elementos { get; set; }
        public DbSet<ElementoProceso> ElementosProceso { get; set; }
        public DbSet<Ficha> Fichas { get; set; }
        public DbSet<Proceso> Procesos { get; set; }
        public DbSet<Programa> Programas { get; set; }
        public DbSet<RegistroNFC> RegistrosNFC { get; set; }
        public DbSet<TipoElemento> TiposElemento { get; set; }
        public DbSet<TipoProceso> TiposProceso { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        
        // Maintain legacy tables for backward compatibility if needed
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Guardia> Guardias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique constraints as per database schema
            modelBuilder.Entity<Aprendiz>()
                .HasIndex(a => a.Correo)
                .IsUnique();
            
            modelBuilder.Entity<Aprendiz>()
                .HasIndex(a => a.NumeroDocumento)
                .IsUnique();
            
            modelBuilder.Entity<Aprendiz>()
                .HasIndex(a => a.CodigoBarras)
                .IsUnique();
            
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();
            
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NumeroDocumento)
                .IsUnique();
            
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.CodigoBarras)
                .IsUnique();
            
            modelBuilder.Entity<Elemento>()
                .HasIndex(e => e.Serial)
                .IsUnique();
            
            modelBuilder.Entity<Elemento>()
                .HasIndex(e => e.CodigoNFC)
                .IsUnique();
            
            modelBuilder.Entity<Ficha>()
                .HasIndex(f => f.Codigo)
                .IsUnique();
            
            modelBuilder.Entity<Programa>()
                .HasIndex(p => p.Codigo)
                .IsUnique();
            
            modelBuilder.Entity<TipoElemento>()
                .HasIndex(t => t.Tipo)
                .IsUnique();
            
            modelBuilder.Entity<TipoProceso>()
                .HasIndex(t => t.Tipo)
                .IsUnique();
        }
    }
}

