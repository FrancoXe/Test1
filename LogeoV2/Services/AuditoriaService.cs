using LogeoV2.Data;
using LogeoV2.Models;
using Microsoft.EntityFrameworkCore;

namespace LogeoV2.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly AppDBContext _context;

        public AuditoriaService(AppDBContext context)
        {
            _context = context;
        }

        public async Task RegistrarAccion(string accion, string entidad, int idEntidad, string? detalleAnterior, string? detalleNuevo, int idUsuarioQueRealizo)
        {
            _context.LogAuditoria.Add(new LogAuditoria
            {
                Accion = accion,
                Entidad = entidad,
                IdEntidad = idEntidad,
                DetalleAnterior = detalleAnterior,
                DetalleNuevo = detalleNuevo,
                FechaAccion = DateTime.UtcNow,
                IdUsuarioQueRealizo = idUsuarioQueRealizo
            });

            await _context.SaveChangesAsync();
        }

        public async Task<List<LogAuditoria>> ObtenerLog(int pagina = 1, int tamanioPagina = 30)
        {
            return await _context.LogAuditoria
                .Include(l => l.UsuarioQueRealizo)
                .OrderByDescending(l => l.FechaAccion)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();
        }
    }
}