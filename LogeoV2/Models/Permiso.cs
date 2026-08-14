using System.ComponentModel.DataAnnotations;

namespace LogeoV2.Models
{
    /// <summary>
    /// Representa un permiso específico en el sistema
    /// </summary>
    public class Permiso
    {
        [Required]
        public int IdPermiso { get; set; }

        [Required(ErrorMessage = "El nombre del permiso es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del permiso debe tener entre 3 y 50 caracteres")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres")]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Categoría del permiso (ej. Usuarios, Configuración, Reportes)
        /// </summary>
        [Required(ErrorMessage = "La categoría del permiso es requerida")]
        [StringLength(50, ErrorMessage = "La categoría no puede superar los 50 caracteres")]
        public string Categoria { get; set; }

        public Permiso()
        {
            Nombre = string.Empty;
            Categoria = string.Empty;
        }
    }

    /// <summary>
    /// Permisos predefinidos del sistema
    /// </summary>
    public static class PermisosDefecto
    {
        // Permisos de usuario
        public const string VerPerfil = "VER_PERFIL";
        public const string EditarPerfil = "EDITAR_PERFIL";
        public const string EliminarPerfil = "ELIMINAR_PERFIL";

        // Permisos de administración
        public const string AdministrarUsuarios = "ADMIN_USUARIOS";
        public const string AdministrarRoles = "ADMIN_ROLES";
        public const string VerRegistros = "VER_REGISTROS";

        // Permisos de sistema
        public const string ConfigurarSistema = "CONFIG_SISTEMA";
        public const string VerReportes = "VER_REPORTES";
    }
}
