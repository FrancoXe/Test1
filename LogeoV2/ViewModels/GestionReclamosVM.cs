using LogeoV2.Models;

namespace LogeoV2.ViewModels
{
    public class GestionReclamosVM
    {
        public List<Reclamo> Reclamos { get; set; } = new();
        public string? Estado { get; set; }
        public List<Reclamo> ReclamosVencidos { get; set; } = new();

        public string? Busqueda { get; set; }

        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public List<string> EstadosDisponibles { get; set; } = new()
        {
            "Pendiente", "Aceptado", "Rechazado", "En Proceso", "Resuelto"
        };
    }
}