using System.ComponentModel.DataAnnotations;

namespace LogeoV2.Models
{
    /// <summary>
    /// Representa la relación entre roles y permisos
    /// </summary>
    public class RolPermiso
    {
        [Required]
        public int IdRolPermiso { get; set; }

        [Required]
        public int IdRol { get; set; }
        public Rol Rol { get; set; }

        [Required]
        public int IdPermiso { get; set; }
        public Permiso Permiso { get; set; }

        /// <summary>
        /// Indica si el permiso está activo para este rol
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha de asignación del permiso
        /// </summary>
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    }
}
