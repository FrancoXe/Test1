using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogeoV2.Data;
using LogeoV2.Services;

namespace LogeoV2.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        private readonly AppDBContext _context;
        private readonly INotificacionService _notificacionService;

        public NotificacionesController(AppDBContext context, INotificacionService notificacionService)
        {
            _context = context;
            _notificacionService = notificacionService;
        }

        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            var usuario = await ObtenerUsuarioActual();
            if (usuario == null) return Unauthorized();

            var notificaciones = await _notificacionService.ObtenerNotificaciones(usuario.IDUsuario);
            var noLeidas = await _notificacionService.ContarNoLeidas(usuario.IDUsuario);

            return Json(new
            {
                noLeidas,
                notificaciones = notificaciones.Select(n => new
                {
                    n.IdNotificacion,
                    n.Titulo,
                    n.Mensaje,
                    fecha = n.FechaCreacion.ToString("g"),
                    n.Leida
                })
            });
        }

        [HttpPost]
        public async Task<IActionResult> MarcarLeida(int idNotificacion)
        {
            var usuario = await ObtenerUsuarioActual();
            if (usuario == null) return Unauthorized();

            var exito = await _notificacionService.MarcarComoLeida(idNotificacion, usuario.IDUsuario);
            return Json(new { success = exito });
        }

        [HttpPost]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var usuario = await ObtenerUsuarioActual();
            if (usuario == null) return Unauthorized();

            await _notificacionService.MarcarTodasComoLeidas(usuario.IDUsuario);
            return Json(new { success = true });
        }

        private async Task<Models.Usuario?> ObtenerUsuarioActual()
        {
            var correo = User.FindFirst("Correo")?.Value;
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
        }
    }
}