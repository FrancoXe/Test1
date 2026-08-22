using LogeoV2.Models;

namespace LogeoV2.Services
{
    public interface IEmailService
    {
        Task EnviarEmail(string destinatario, string asunto, string cuerpo);
        Task EnviarEmailCambioEstado(string destinatario, Reclamo reclamo, string estadoAnterior, string estadoNuevo, string urlBase);
    }
}