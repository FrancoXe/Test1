using System.Net;
using System.Net.Mail;

namespace LogeoV2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task EnviarEmail(string destinatario, string asunto, string cuerpo)
        {
            try
            {
                var smtpServer = _config["EmailSettings:SmtpServer"];
                var puerto = int.Parse(_config["EmailSettings:Puerto"]!);
                var correo = _config["EmailSettings:Correo"];
                var contrasena = _config["EmailSettings:ContrasenaApp"];
                var nombreRemitente = _config["EmailSettings:NombreRemitente"];

                using var mensaje = new MailMessage
                {
                    From = new MailAddress(correo!, nombreRemitente),
                    Subject = asunto,
                    Body = cuerpo,
                    IsBodyHtml = false
                };
                mensaje.To.Add(destinatario);

                using var cliente = new SmtpClient(smtpServer, puerto)
                {
                    Credentials = new NetworkCredential(correo, contrasena),
                    EnableSsl = true
                };

                await cliente.SendMailAsync(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email a {destinatario}");
                // No relanzamos la excepción: que falle el mail no debe romper el flujo del reclamo
            }
        }
    }
}