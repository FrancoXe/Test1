namespace LogeoV2.Services
{
    public interface IEmailService
    {
        Task EnviarEmail(string destinatario, string asunto, string cuerpo);
    }
}