using LogeoV2.Models;

namespace LogeoV2.Services
{
    public interface IPermisoService
    {
        /// <summary>
        /// Inicializa los permisos predeterminados en el sistema
        /// </summary>
        Task<int> InicializarPermisosPredeterminados();

        /// <summary>
        /// Asigna un conjunto de permisos a un rol
        /// </summary>
        Task AsignarPermisosARol(string nombreRol, string[] permisos);

        /// <summary>
        /// Verifica si un rol tiene un permiso específico
        /// </summary>
        Task<bool> RolTienePermiso(string nombreRol, string nombrePermiso);

        /// <summary>
        /// Obtiene todos los permisos de un rol
        /// </summary>
        Task<IEnumerable<Permiso>> ObtenerPermisosPorRol(string nombreRol);

        /// <summary>
        /// Obtiene todos los permisos disponibles en el sistema
        /// </summary>
        Task<IEnumerable<Permiso>> ObtenerTodosLosPermisos();
    }
}
