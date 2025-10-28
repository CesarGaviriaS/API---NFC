using Microsoft.EntityFrameworkCore;
using API___NFC.Models;

namespace ApiNfc.Data
{
    public class NfcDbContext : DbContext
    {
        public NfcDbContext(DbContextOptions<NfcDbContext> options) : base(options) { }

        public DbSet<Aprendiz> Aprendices { get; set; }
        public DbSet<Elemento> Elementos { get; set; }
        public DbSet<ElementoProceso> ElementoProcesos { get; set; }
        public DbSet<Ficha> Fichas { get; set; }
        public DbSet<Proceso> Procesos { get; set; }
        public DbSet<Programa> Programas { get; set; }
        public DbSet<RegistroNFC> RegistrosNFC { get; set; }
        public DbSet<TipoElemento> TipoElementos { get; set; }
        public DbSet<TipoProceso> TipoProcesos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aprendiz
            modelBuilder.Entity<Aprendiz>(entity =>
            {
                entity.ToTable("Aprendiz");
                entity.HasKey(e => e.IdAprendiz);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                entity.Property(e => e.TipoDocumento).IsRequired().HasMaxLength(5).HasColumnType("varchar(5)");
                entity.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(20).HasColumnType("varchar(20)");
                entity.Property(e => e.Correo).IsRequired().HasMaxLength(150).HasColumnType("varchar(150)");
                entity.Property(e => e.CodigoBarras).IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                entity.Property(e => e.Telefono).HasMaxLength(20).HasColumnType("varchar(20)").IsRequired(false);
                entity.Property(e => e.FotoUrl).HasMaxLength(255).HasColumnType("varchar(255)").IsRequired(false);
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("getdate()");
                entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("getdate()");
                entity.HasIndex(e => e.Correo).IsUnique();
                entity.HasIndex(e => e.NumeroDocumento).IsUnique();
                entity.HasIndex(e => e.CodigoBarras).IsUnique();
                entity.HasOne(e => e.Ficha).WithMany(f => f.Aprendices).HasForeignKey(e => e.IdFicha).OnDelete(DeleteBehavior.Restrict);
            });

            // Elemento
            modelBuilder.Entity<Elemento>(entity =>
            {
                entity.ToTable("Elemento");
                entity.HasKey(e => e.IdElemento);
                entity.Property(e => e.IdTipoElemento).IsRequired();
                entity.Property(e => e.IdPropietario).IsRequired();
                entity.Property(e => e.TipoPropietario).IsRequired().HasMaxLength(20).HasColumnType("varchar(20)");
                entity.Property(e => e.Marca).HasMaxLength(100).HasColumnType("varchar(100)").IsRequired(false);
                entity.Property(e => e.Modelo).HasMaxLength(100).HasColumnType("varchar(100)").IsRequired(false);
                entity.Property(e => e.Serial).IsRequired().HasMaxLength(150).HasColumnType("varchar(150)");
                entity.Property(e => e.CodigoNFC).HasMaxLength(100).HasColumnType("varchar(100)").IsRequired(false);
                entity.Property(e => e.Descripcion).HasColumnType("text").IsRequired(false);
                entity.Property(e => e.ImagenUrl).HasMaxLength(255).HasColumnType("varchar(255)").IsRequired(false);
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("getdate()");
                entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("getdate()");
                entity.HasIndex(e => e.Serial).IsUnique();
                entity.HasIndex(e => e.CodigoNFC).IsUnique();
                entity.HasOne(e => e.TipoElemento).WithMany(t => t.Elementos).HasForeignKey(e => e.IdTipoElemento).OnDelete(DeleteBehavior.Restrict);
            });

            // ElementoProceso
            modelBuilder.Entity<ElementoProceso>(entity =>
            {
                entity.ToTable("ElementoProceso");
                entity.HasKey(e => e.IdElementoProceso);
                entity.Property(e => e.Validado).HasDefaultValue(false);
                entity.HasOne(e => e.Elemento).WithMany().HasForeignKey(e => e.IdElemento).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Proceso).WithMany(p => p.ElementoProcesos).HasForeignKey(e => e.IdProceso).OnDelete(DeleteBehavior.Cascade);
            });

            // Ficha
            modelBuilder.Entity<Ficha>(entity =>
            {
                entity.ToTable("Ficha");
                entity.HasKey(e => e.IdFicha);
                entity.Property(e => e.IdPrograma).IsRequired();
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
                entity.Property(e => e.FechaInicio).HasColumnType("date");
                entity.Property(e => e.FechaFinal).HasColumnType("date");
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("getdate()");
                entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("getdate()");
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasOne(e => e.Programa).WithMany(p => p.Fichas).HasForeignKey(e => e.IdPrograma).OnDelete(DeleteBehavior.Restrict);
            });

            // Proceso
            modelBuilder.Entity<Proceso>(entity =>
            {
                entity.ToTable("Proceso");
                entity.HasKey(e => e.IdProceso);
                entity.Property(e => e.IdTipoProceso).IsRequired();
                entity.Property(e => e.TipoPersona).IsRequired().HasMaxLength(20).HasColumnType("varchar(20)");
                entity.Property(e => e.IdGuardia).IsRequired();
                entity.Property(e => e.TimeStampEntradaSalida).HasDefaultValueSql("getdate()");
                entity.Property(e => e.RequiereOtrosProcesos).HasDefaultValue(false);
                entity.Property(e => e.Observaciones).HasColumnType("text").IsRequired(false);
                entity.Property(e => e.SincronizadoBD).HasDefaultValue(false);
                entity.HasOne(e => e.Aprendiz).WithMany().HasForeignKey(e => e.IdAprendiz).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.TipoProceso).WithMany(t => t.Procesos).HasForeignKey(e => e.IdTipoProceso).OnDelete(DeleteBehavior.Restrict);
            });

            // Programa
            modelBuilder.Entity<Programa>(entity =>
            {
                entity.ToTable("Programa");
                entity.HasKey(e => e.IdPrograma);
                entity.Property(e => e.NombrePrograma).IsRequired().HasMaxLength(200).HasColumnType("varchar(200)");
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
                entity.Property(e => e.NivelFormacion).IsRequired().HasMaxLength(30).HasColumnType("varchar(30)");
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("getdate()");
                entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("getdate()");
                entity.HasIndex(e => e.Codigo).IsUnique();
            });

            // RegistroNFC
            modelBuilder.Entity<RegistroNFC>(entity =>
            {
                entity.ToTable("RegistroNFC");
                entity.HasKey(e => e.IdRegistro);
                entity.Property(e => e.TipoRegistro).IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("getdate()");
                entity.Property(e => e.Estado).HasMaxLength(20).HasColumnType("nvarchar(20)").IsRequired(false);
                entity.HasOne(r => r.Aprendiz).WithMany().HasForeignKey(r => r.IdAprendiz).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(r => r.Usuario).WithMany().HasForeignKey(r => r.IdUsuario).OnDelete(DeleteBehavior.Restrict);
            });

            // TipoElemento
            modelBuilder.Entity<TipoElemento>(entity =>
            {
                entity.ToTable("TipoElemento");
                entity.HasKey(e => e.IdTipoElemento);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                entity.Property(e => e.RequiereNFC).HasDefaultValue(false);
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("getdate()");
                entity.HasIndex(e => e.Tipo).IsUnique();
            });

            // TipoProceso
            modelBuilder.Entity<TipoProceso>(entity =>
            {
                entity.ToTable("TipoProceso");
                entity.HasKey(e => e.IdTipoProceso);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.HasIndex(e => e.Tipo).IsUnique();
            });

            // Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(e => e.IdUsuario);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                entity.Property(e => e.TipoDocumento).IsRequired().HasMaxLength(5).HasColumnType("varchar(5)");
                entity.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(20).HasColumnType("varchar(20)");
                entity.Property(e => e.Correo).IsRequired().HasMaxLength(150).HasColumnType("varchar(150)");
                entity.Property(e => e.Contrasena).IsRequired().HasMaxLength(255).HasColumnName("Contraseña").HasColumnType("varchar(255)");
                entity.Property(e => e.Rol).IsRequired().HasMaxLength(20).HasColumnType("varchar(20)");
                entity.Property(e => e.CodigoBarras).IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                entity.Property(e => e.Cargo).HasMaxLength(100).HasColumnType("varchar(100)").IsRequired(false);
                entity.Property(e => e.Telefono).HasMaxLength(20).HasColumnType("varchar(20)").IsRequired(false);
                entity.Property(e => e.FotoUrl).HasMaxLength(255).HasColumnType("varchar(255)").IsRequired(false);
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("getdate()");
                entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("getdate()");
                entity.HasIndex(e => e.Correo).IsUnique();
                entity.HasIndex(e => e.NumeroDocumento).IsUnique();
                entity.HasIndex(e => e.CodigoBarras).IsUnique();
            });
        }
    }
}