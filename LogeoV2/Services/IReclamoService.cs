using LogeoV2.Models;
using LogeoV2.ViewModels;

namespace LogeoV2.Services
{
    public interface IReclamoService
    {
        Task<Reclamo> CrearReclamo(Reclamo reclamo, IFormFile? archivo);
        Task<List<Categoria>> ObtenerCategorias();
        Task<List<Subcategoria>> ObtenerSubcategorias(int idCategoria);
        Task<List<Barrio>> ObtenerBarrios();
        Task<ResultadoPaginado<Reclamo>> ObtenerReclamos(string? estado, string? busqueda, int pagina = 1, int tamanioPagina = 20);
        Task<bool> CambiarEstadoReclamo(int idReclamo, string nuevoEstado, int idUsuarioQueCambia, bool notificar = true);
        Task<ResultadoPaginado<Reclamo>> ObtenerHistorial(string? estado, int? idCategoria, int? idBarrio, DateTime? fechaDesde, DateTime? fechaHasta, string? busqueda, int pagina = 1, int tamanioPagina = 20);
        Task<MetricasReclamosVM> ObtenerMetricas(DateTime? desde, DateTime? hasta);
        Task<List<int>> ObtenerAniosDisponibles();
        Task<TendenciaVM> ObtenerTendencia(List<string> periodos);
        Task<List<Reclamo>> ObtenerReclamosVencidos();
        Task<List<HistorialEstado>> ObtenerHistorialEstados(int idReclamo);
    }
}