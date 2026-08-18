namespace LogeoV2.ViewModels
{
    public class MetricasReclamosVM
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int TotalReclamos { get; set; }
        public int TotalResueltos { get; set; }
        public int TotalPendientes { get; set; }
        public double TiempoPromedioResolucionDias { get; set; }
        public List<MetricaDesgloseVM> PorCategoria { get; set; } = new();
        public List<MetricaDesgloseVM> PorDepartamento { get; set; } = new();
        public List<MetricaDesgloseVM> PorBarrio { get; set; } = new();
        public List<MetricaDesgloseVM> PorDepartamentoPendientes { get; set; } = new();
        public List<EstadoBarrioVM> EstadoPorBarrio { get; set; } = new();
        public NotificacionesResumenVM Notificaciones { get; set; } = new();
        public List<MotivoPorBarrioVM> MotivoPorBarrio { get; set; } = new();
    }

    public class MetricaDesgloseVM
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double PromedioDias { get; set; }
    }

    public class TendenciaVM
    {
        public List<string> Labels { get; set; } = new();
        public List<SerieTendenciaVM> Series { get; set; } = new();
    }

    public class SerieTendenciaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public List<int> Datos { get; set; } = new();
    }

    public class EstadoBarrioVM
    {
        public string Barrio { get; set; } = string.Empty;
        public int Pendiente { get; set; }
        public int EnProceso { get; set; }
        public int Resuelto { get; set; }
        public int Rechazado { get; set; }
        public int Aceptado { get; set; }
    }

    public class NotificacionesResumenVM
    {
        public int TotalReclamos { get; set; }
        public int Notificados { get; set; }
        public int NoNotificados { get; set; }
    }

    public class MotivoPorBarrioVM
    {
        public string Barrio { get; set; } = string.Empty;
        public List<CategoriaCantidadVM> Categorias { get; set; } = new();
    }

    public class CategoriaCantidadVM
    {
        public string Categoria { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}