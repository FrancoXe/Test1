using LogeoV2.Models;

namespace LogeoV2.Services
{
    public interface INotificacionService
    {
        Task CrearNotificacion(int idUsuario, string titulo, string mensaje, bool enviarEmail = true);
        Task<List<Notificacion>> ObtenerNotificaciones(int idUsuario);
        Task<int> ContarNoLeidas(int idUsuario);
        Task<bool> MarcarComoLeida(int idNotificacion, int idUsuario);
        Task MarcarTodasComoLeidas(int idUsuario);
    }
}