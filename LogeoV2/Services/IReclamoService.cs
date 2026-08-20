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
        Task<List<Reclamo>> ObtenerReclamos(string? estado);
        Task<bool> CambiarEstadoReclamo(int idReclamo, string nuevoEstado, bool notificar = true);
        Task<List<Reclamo>> ObtenerHistorial(string? estado, int? idCategoria, int? idBarrio, DateTime? fechaDesde, DateTime? fechaHasta, string? busqueda);

        Task<MetricasReclamosVM> ObtenerMetricas(DateTime? desde, DateTime? hasta);
        Task<List<int>> ObtenerAniosDisponibles();
        Task<TendenciaVM> ObtenerTendencia(List<string> periodos);

        Task<List<Reclamo>> ObtenerReclamosVencidos();
    }
}