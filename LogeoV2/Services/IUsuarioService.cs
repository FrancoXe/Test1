using LogeoV2.Models;

namespace LogeoV2.Services
{
    public interface IUsuarioService
    {
        /// <summary>
        /// Obtiene usuarios con búsqueda y orden opcionales
        /// </summary>
        Task<IEnumerable<Usuario>> ObtenerUsuarios(string? busqueda, string? ordenarPor, bool ascendente);

        /// <summary>
        /// Cambia el rol de un usuario
        /// </summary>
        Task<bool> CambiarRolUsuario(int idUsuario, int idRol);
    }
}