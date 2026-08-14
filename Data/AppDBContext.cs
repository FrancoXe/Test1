using Microsoft.EntityFrameworkCore;
using LogeoV2.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace LogeoV2.Data
{
    public class AppDBContext : DbContext
    {
        private readonly ILogger<AppDBContext> _logger;

        public AppDBContext(
            DbContextOptions<AppDBContext> options, 
            ILogger<AppDBContext> logger) : base(options)
        {
            _logger = logger;
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }

        public override int SaveChanges()
        {
            try 
            {
                ValidateEntities();
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios");
                throw;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try 
            {
                ValidateEntities();
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios asincrónicamente");
                throw;
            }
        }

        private void ValidateEntities()
        {
            var entities = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity);

            foreach (var entity in entities)
            {
                // Solo validar si la entidad tiene atributos de validación
                if (ShouldValidateEntity(entity))
                {
                    ValidateEntity(entity);
                }
            }
        }

        private bool ShouldValidateEntity(object entity)
        {
            var properties = entity.GetType().GetProperties();
            return properties.Any(p => p.GetCustomAttributes(typeof(RequiredAttribute), true).Any());
        }

        private void ValidateEntity(object entity)
        {
            var validationContext = new ValidationContext(entity, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(entity, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage);
                var errorMessage = string.Join("; ", errors);
                
                _logger.LogError("Validación fallida para entidad {EntityType}: {Errors}", 
                    entity.GetType().Name, errorMessage);

                throw new ValidationException($"Validación de entidad fallida: {errorMessage}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol>(tb =>
            {
                tb.ToTable("Roles");
                tb.HasKey(x => x.IdRol);
                tb.Property(x => x.IdRol)
                    .HasColumnName("IdRol")
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();
                tb.Property(x => x.NombreRol)
                    .HasColumnName("NombreRol")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            modelBuilder.Entity<Usuario>(tb =>
            {
                tb.ToTable("Usuarios");
                tb.HasKey(x => x.IDUsuario);
                tb.Property(x => x.IDUsuario)
                    .HasColumnName("IDUsuario")
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();

                tb.Property(x => x.Nombre)
                    .HasColumnName("Nombre")
                    .HasMaxLength(50)
                    .IsRequired();

                tb.Property(x => x.Apellido)
                    .HasColumnName("Apellido")
                    .HasMaxLength(50)
                    .IsRequired();

                tb.Property(x => x.Correo)
                    .HasColumnName("Correo")
                    .HasMaxLength(100)
                    .IsRequired();

                tb.Property(x => x.Clave)
                    .HasColumnName("Clave")
                    .HasMaxLength(100)
                    .IsRequired();

                tb.Property(x => x.IdRol)
                    .HasColumnName("IdRol")
                    .IsRequired();

                tb.Property(x => x.UltimoAcceso)
                    .HasColumnName("UltimoAcceso")
                    .IsRequired(false);

                // Configurar relación con Rol
                tb.HasOne(u => u.Rol)
                  .WithMany()
                  .HasForeignKey(u => u.IdRol)
                  .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Permiso>(tb =>
            {
                tb.ToTable("Permisos");
                tb.HasKey(x => x.IdPermiso);
                tb.Property(x => x.IdPermiso)
                    .HasColumnName("IdPermiso")
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();
                
                tb.Property(x => x.Nombre)
                    .HasColumnName("Nombre")
                    .HasMaxLength(50)
                    .IsRequired();
                
                tb.Property(x => x.Descripcion)
                    .HasColumnName("Descripcion")
                    .HasMaxLength(200);
                
                tb.Property(x => x.Categoria)
                    .HasColumnName("Categoria")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            modelBuilder.Entity<RolPermiso>(tb =>
            {
                tb.ToTable("RolPermisos");
                tb.HasKey(x => x.IdRolPermiso);
                
                tb.Property(x => x.IdRolPermiso)
                    .HasColumnName("IdRolPermiso")
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();
                
                tb.HasOne(rp => rp.Rol)
                    .WithMany()
                    .HasForeignKey(rp => rp.IdRol)
                    .OnDelete(DeleteBehavior.Restrict);
                
                tb.HasOne(rp => rp.Permiso)
                    .WithMany()
                    .HasForeignKey(rp => rp.IdPermiso)
                    .OnDelete(DeleteBehavior.Restrict);
                
                tb.Property(x => x.Activo)
                    .HasColumnName("Activo")
                    .IsRequired();
                
                tb.Property(x => x.FechaAsignacion)
                    .HasColumnName("FechaAsignacion")
                    .IsRequired();
            });
        }
    }
}
