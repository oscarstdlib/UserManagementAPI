using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Models;

namespace UserManagementAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Permiso> Permisos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Usuario entity
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Pais).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TipoUsuario).IsRequired().HasMaxLength(20);
                
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Permiso entity
            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Descripcion).HasMaxLength(200);
                entity.Property(e => e.Recurso).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Accion).IsRequired().HasMaxLength(20);
            });

            // Configure many-to-many relationship
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Permisos)
                .WithMany(p => p.Usuarios)
                .UsingEntity(j => j.ToTable("UsuarioPermisos"));

            // Seed data for permissions
            SeedPermisos(modelBuilder);
        }

        private void SeedPermisos(ModelBuilder modelBuilder)
        {
            var permisos = new[]
            {
                new Permiso { Id = 1, Nombre = "Leer Usuarios", Descripcion = "Permite leer información de usuarios", Recurso = "usuarios", Accion = "GET", FechaCreacion = new DateTime(2024, 1, 1) },
                new Permiso { Id = 2, Nombre = "Crear Usuarios", Descripcion = "Permite crear nuevos usuarios", Recurso = "usuarios", Accion = "POST", FechaCreacion = new DateTime(2024, 1, 1) },
                new Permiso { Id = 3, Nombre = "Actualizar Usuarios", Descripcion = "Permite actualizar información de usuarios", Recurso = "usuarios", Accion = "PUT", FechaCreacion = new DateTime(2024, 1, 1) },
                new Permiso { Id = 4, Nombre = "Eliminar Usuarios", Descripcion = "Permite eliminar usuarios", Recurso = "usuarios", Accion = "DELETE", FechaCreacion = new DateTime(2024, 1, 1) },
                new Permiso { Id = 5, Nombre = "Leer Permisos", Descripcion = "Permite leer información de permisos", Recurso = "permisos", Accion = "GET", FechaCreacion = new DateTime(2024, 1, 1) },
                new Permiso { Id = 6, Nombre = "Crear Permisos", Descripcion = "Permite crear nuevos permisos", Recurso = "permisos", Accion = "POST", FechaCreacion = new DateTime(2024, 1, 1) },
                new Permiso { Id = 7, Nombre = "Actualizar Permisos", Descripcion = "Permite actualizar información de permisos", Recurso = "permisos", Accion = "PUT", FechaCreacion = new DateTime(2024, 1, 1) },
                new Permiso { Id = 8, Nombre = "Eliminar Permisos", Descripcion = "Permite eliminar permisos", Recurso = "permisos", Accion = "DELETE", FechaCreacion = new DateTime(2024, 1, 1) },
                new Permiso { Id = 9, Nombre = "Leer Países", Descripcion = "Permite leer información de países", Recurso = "paises", Accion = "GET", FechaCreacion = new DateTime(2024, 1, 1) }
            };

            modelBuilder.Entity<Permiso>().HasData(permisos);
        }
    }
} 