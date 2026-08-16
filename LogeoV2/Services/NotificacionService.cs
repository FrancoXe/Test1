using LogeoV2.Data;
using LogeoV2.Models;
using Microsoft.EntityFrameworkCore;

namespace LogeoV2.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly AppDBContext _context;
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificacionService(AppDBContext context, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _scopeFactory = scopeFactory;
        }

        public async Task CrearNotificacion(int idUsuario, string titulo, string mensaje, bool enviarEmail = true)
        {
            var notificacion = new Notificacion
            {
                IdUsuario = idUsuario,
                Titulo = titulo,
                Mensaje = mensaje,
                FechaCreacion = DateTime.UtcNow,
                Leida = false
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            if (enviarEmail)
            {
                var usuario = await _context.Usuarios.FindAsync(idUsuario);
                if (usuario != null)
                {
                    var correo = usuario.Correo;
                    _ = Task.Run(async () =>
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.EnviarEmail(correo, titulo, mensaje);
                    });
                }
            }
        }

        public async Task<List<Notificacion>> ObtenerNotificaciones(int idUsuario)
        {
            return await _context.Notificaciones
                .Where(n => n.IdUsuario == idUsuario)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(20)
                .ToListAsync();
        }

        public async Task<int> ContarNoLeidas(int idUsuario)
        {
            return await _context.Notificaciones
                .CountAsync(n => n.IdUsuario == idUsuario && !n.Leida);
        }

        public async Task<bool> MarcarComoLeida(int idNotificacion, int idUsuario)
        {
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.IdNotificacion == idNotificacion && n.IdUsuario == idUsuario);

            if (notificacion == null)
                return false;

            notificacion.Leida = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task MarcarTodasComoLeidas(int idUsuario)
        {
            var noLeidas = await _context.Notificaciones
                .Where(n => n.IdUsuario == idUsuario && !n.Leida)
                .ToListAsync();

            foreach (var n in noLeidas)
                n.Leida = true;

            await _context.SaveChangesAsync();
        }
    }
}