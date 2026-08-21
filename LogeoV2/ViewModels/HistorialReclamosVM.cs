using LogeoV2.Models;

namespace LogeoV2.ViewModels
{
    public class HistorialReclamosVM
    {
        public List<Reclamo> Reclamos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();
        public List<Barrio> Barrios { get; set; } = new();
        public List<string> EstadosDisponibles { get; set; } = new()
        {
            "Pendiente", "Aceptado", "Rechazado", "En Proceso", "Resuelto"
        };

        public string? Estado { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdBarrio { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Busqueda { get; set; }

        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
    }
}