using LogeoV2.Models;

namespace LogeoV2.Services
{
    public interface IAuditoriaService
    {
        Task RegistrarAccion(string accion, string entidad, int idEntidad, string? detalleAnterior, string? detalleNuevo, int idUsuarioQueRealizo);
        Task<List<LogAuditoria>> ObtenerLog(int pagina = 1, int tamanioPagina = 30);
    }
}