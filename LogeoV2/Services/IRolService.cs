using LogeoV2.Models;

namespace LogeoV2.Services
{
    public interface IRolService
    {
        /// <summary>
        /// Inicializa los roles predeterminados en el sistema
        /// </summary>
        Task<int> InicializarRolesPredeterminados();

        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        Task<Rol?> ObtenerRolPorNombre(string nombreRol);

        /// <summary>
        /// Crea un nuevo rol si no existe
        /// </summary>
        Task<Rol> CrearRolSiNoExiste(string nombreRol);

        /// <summary>
        /// Obtiene todos los roles del sistema
        /// </summary>
        Task<IEnumerable<Rol>> ObtenerTodosLosRoles();

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        Task<Rol?> ObtenerRolPorId(int idRol);
    }
}
