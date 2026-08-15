using LogeoV2.Models;

namespace LogeoV2.ViewModels
{
    public class GestionReclamosVM
    {
        public List<Reclamo> Reclamos { get; set; } = new();
        public string? Estado { get; set; }
        public List<string> EstadosDisponibles { get; set; } = new()
        {
            "Pendiente", "Aceptado", "Rechazado", "En Proceso", "Resuelto"
        };
    }
}