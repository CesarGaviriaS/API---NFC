using API___NFC.Models;
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
        public DbSet<Proceso> Procesos { get; set; }
        public DbSet<Elemento> Elementos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Aprendiz> Aprendices { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }

        public DbSet<Ficha> Fichas { get; set; }
        public DbSet<Programa> Programas { get; set; }
        public DbSet<TipoElemento> TiposElemento { get; set; }
        public DbSet<TipoProceso> TiposProceso { get; set; }
        public DbSet<Guardia> Guardias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Elementos) // Un Usuario tiene muchos Elementos...
                .WithOne(e => e.Propietario) // ...y cada Elemento tiene un Propietario...
                .HasForeignKey(e => e.IdPropietario); // ...usando la clave foránea 'IdPropietario'.
        }
    }
}

