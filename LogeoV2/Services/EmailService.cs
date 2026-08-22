using LogeoV2.Models;
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
                _logger.LogInformation($"Email enviado correctamente a {destinatario}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email a {destinatario}");
            }
        }

        public async Task EnviarEmailCambioEstado(string destinatario, Reclamo reclamo, string estadoAnterior, string estadoNuevo, string urlBase)
        {
            try
            {
                var mensajeAdicional = estadoNuevo switch
                {
                    "Aceptado" => "Tu reclamo fue aceptado y asignado al área correspondiente.",
                    "Rechazado" => "Lamentablemente tu reclamo no pudo ser aceptado. Podés contactarte con la Municipalidad para más información.",
                    "En Proceso" => "Tu reclamo está siendo gestionado por el área correspondiente.",
                    "Resuelto" => "¡Tu reclamo fue resuelto! Gracias por ayudarnos a mejorar el municipio.",
                    _ => ""
                };

                var urlDetalle = $"{urlBase}/Reclamos/VerDetallePdf?id={reclamo.IdReclamo}";

                var cuerpoHtml = $@"
<!DOCTYPE html>
<html>
<body style=""font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px;"">
    <div style=""max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        <div style=""background-color: #000000; padding: 20px; text-align: center; border-bottom: 3px solid #fd7e14;"">
            <h2 style=""color: #ffffff; margin: 0; font-size: 18px;"">Municipalidad de Unquillo</h2>
        </div>
        <div style=""padding: 24px;"">
            <h3 style=""color: #212529; margin-top: 0;"">Actualización de tu reclamo</h3>
            <p style=""color: #495057;"">Reclamo <strong>#{reclamo.IdReclamo}</strong></p>
            <table style=""width: 100%; border-collapse: collapse; margin: 16px 0;"">
                <tr>
                    <td style=""padding: 6px 0; color: #6c757d; font-size: 14px;"">Categoría:</td>
                    <td style=""padding: 6px 0; color: #212529; font-size: 14px;""><strong>{reclamo.Categoria?.Nombre}</strong></td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #6c757d; font-size: 14px; vertical-align: top;"">Descripción:</td>
                    <td style=""padding: 6px 0; color: #212529; font-size: 14px;"">{reclamo.Descripcion}</td>
                </tr>
            </table>
            <div style=""background-color: #fff5eb; border-left: 4px solid #fd7e14; padding: 12px 16px; margin: 16px 0; border-radius: 4px;"">
                <p style=""margin: 0; color: #495057; font-size: 14px;"">
                    Estado: <span style=""text-decoration: line-through; color: #adb5bd;"">{estadoAnterior}</span>
                    → <strong style=""color: #fd7e14;"">{estadoNuevo}</strong>
                </p>
            </div>
            <p style=""color: #495057; font-size: 14px;"">{mensajeAdicional}</p>
            <p style=""color: #adb5bd; font-size: 12px;"">Actualizado el {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            <div style=""text-align: center; margin: 24px 0;"">
                <a href=""{urlDetalle}"" style=""background-color: #fd7e14; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 6px; font-weight: bold; display: inline-block;"">
                    Ver mi reclamo
                </a>
            </div>
        </div>
        <div style=""background-color: #f8f9fa; padding: 16px 24px; text-align: center; border-top: 1px solid #dee2e6;"">
            <p style=""color: #6c757d; font-size: 12px; margin: 0;"">
                Este es un mensaje automático. Por favor, no respondas a este correo.<br />
                Municipalidad de Unquillo · Sistema de Gestión de Reclamos
            </p>
        </div>
    </div>
</body>
</html>";

                var smtpServer = _config["EmailSettings:SmtpServer"];
                var puerto = int.Parse(_config["EmailSettings:Puerto"]!);
                var correo = _config["EmailSettings:Correo"];
                var contrasena = _config["EmailSettings:ContrasenaApp"];
                var nombreRemitente = _config["EmailSettings:NombreRemitente"];

                using var mensaje = new MailMessage
                {
                    From = new MailAddress(correo!, nombreRemitente),
                    Subject = $"Actualización de tu reclamo #{reclamo.IdReclamo}",
                    Body = cuerpoHtml,
                    IsBodyHtml = true
                };
                mensaje.To.Add(destinatario);

                using var cliente = new SmtpClient(smtpServer, puerto)
                {
                    Credentials = new NetworkCredential(correo, contrasena),
                    EnableSsl = true
                };

                await cliente.SendMailAsync(mensaje);
                _logger.LogInformation($"Email de cambio de estado enviado correctamente a {destinatario}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email de cambio de estado a {destinatario}");
            }
        }
    }
}