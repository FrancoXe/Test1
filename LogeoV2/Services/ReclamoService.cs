using LogeoV2.Data;
using LogeoV2.Models;
using LogeoV2.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogeoV2.Services
{
    public class ReclamoService : IReclamoService
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ReclamoService> _logger;
        private readonly INotificacionService _notificacionService;

        private static readonly string[] EstadosValidos = { "Pendiente", "Aceptado", "Rechazado", "En Proceso", "Resuelto" };

        public ReclamoService(
            AppDBContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ReclamoService> logger,
            INotificacionService notificacionService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _notificacionService = notificacionService;
        }

        public async Task<Reclamo> CrearReclamo(Reclamo reclamo, IFormFile? archivo)
        {
            if (archivo != null && archivo.Length > 0)
            {
                if (archivo.Length > 5 * 1024 * 1024)
                    throw new InvalidOperationException("El archivo no puede superar los 5MB");

                var carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(carpeta);

                var nombreUnico = $"{Guid.NewGuid()}_{archivo.FileName}";
                var rutaFisica = Path.Combine(carpeta, nombreUnico);

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                reclamo.RutaArchivo = "/uploads/" + nombreUnico;
            }

            reclamo.FechaCreacion = DateTime.UtcNow;
            reclamo.Estado = "Pendiente";

            _context.Reclamos.Add(reclamo);
            await _context.SaveChangesAsync();
            return reclamo;
        }

        public async Task<List<Categoria>> ObtenerCategorias() =>
            await _context.Categorias.ToListAsync();

        public async Task<List<Subcategoria>> ObtenerSubcategorias(int idCategoria) =>
            await _context.Subcategorias.Where(s => s.IdCategoria == idCategoria).ToListAsync();

        public async Task<List<Barrio>> ObtenerBarrios() =>
            await _context.Barrios.ToListAsync();

        public async Task<List<Reclamo>> ObtenerReclamos(string? estado)
        {
            var query = _context.Reclamos
                .Include(r => r.Usuario)
                .Include(r => r.Categoria)
                .Include(r => r.Subcategoria)
                .Include(r => r.Barrio)
                .Include(r => r.DepartamentoAsignado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(r => r.Estado == estado);
            }

            return await query.OrderByDescending(r => r.FechaCreacion).ToListAsync();
        }

        public async Task<bool> CambiarEstadoReclamo(int idReclamo, string nuevoEstado, bool notificar = true)
        {
            if (!EstadosValidos.Contains(nuevoEstado))
                return false;

            var reclamo = await _context.Reclamos
                .Include(r => r.Categoria)
                .FirstOrDefaultAsync(r => r.IdReclamo == idReclamo);

            if (reclamo == null)
                return false;

            reclamo.Estado = nuevoEstado;
            reclamo.FechaActualizacion = DateTime.UtcNow;

            if (nuevoEstado == "Aceptado" && reclamo.Categoria?.IdDepartamento != null)
            {
                reclamo.IdDepartamentoAsignado = reclamo.Categoria.IdDepartamento;
                _logger.LogInformation($"Reclamo {idReclamo} asignado automáticamente al departamento {reclamo.IdDepartamentoAsignado}");
            }

            await _context.SaveChangesAsync();

            if (notificar)
            {
                await _notificacionService.CrearNotificacion(
                    reclamo.IdUsuario,
                    $"Reclamo #{reclamo.IdReclamo} - {nuevoEstado}",
                    $"Tu reclamo fue actualizado al estado: {nuevoEstado}."
                );
            }

            return true;
        }

        public async Task<List<Reclamo>> ObtenerHistorial(string? estado, int? idCategoria, int? idBarrio, DateTime? fechaDesde, DateTime? fechaHasta, string? busqueda)
        {
            var query = _context.Reclamos
                .Include(r => r.Usuario)
                .Include(r => r.Categoria)
                .Include(r => r.Subcategoria)
                .Include(r => r.Barrio)
                .Include(r => r.DepartamentoAsignado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(r => r.Estado == estado);

            if (idCategoria.HasValue)
                query = query.Where(r => r.IdCategoria == idCategoria.Value);

            if (idBarrio.HasValue)
                query = query.Where(r => r.IdBarrio == idBarrio.Value);

            if (fechaDesde.HasValue)
                query = query.Where(r => r.FechaCreacion >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(r => r.FechaCreacion <= fechaHasta.Value.AddDays(1).AddTicks(-1));

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var termino = busqueda.Trim().ToLower();
                query = query.Where(r =>
                    r.DNI.ToLower().Contains(termino) ||
                    r.Usuario!.Nombre.ToLower().Contains(termino) ||
                    r.Usuario!.Apellido.ToLower().Contains(termino));
            }

            return await query.OrderByDescending(r => r.FechaCreacion).ToListAsync();
        }

        public async Task<MetricasReclamosVM> ObtenerMetricas(DateTime? desde, DateTime? hasta)
        {
            var query = _context.Reclamos
                .Include(r => r.Categoria)
                .Include(r => r.Barrio)
                .Include(r => r.DepartamentoAsignado)
                .AsQueryable();

            if (desde.HasValue)
                query = query.Where(r => r.FechaCreacion >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(r => r.FechaCreacion <= hasta.Value.AddDays(1).AddTicks(-1));

            var reclamos = await query.ToListAsync();
            var resueltos = reclamos.Where(r => r.Estado == "Resuelto" && r.FechaActualizacion.HasValue).ToList();

            var vm = new MetricasReclamosVM
            {
                FechaDesde = desde,
                FechaHasta = hasta,
                TotalReclamos = reclamos.Count,
                TotalResueltos = resueltos.Count,
                TotalPendientes = reclamos.Count(r => r.Estado != "Resuelto"),
                TiempoPromedioResolucionDias = resueltos.Any()
                    ? resueltos.Average(r => (r.FechaActualizacion!.Value - r.FechaCreacion).TotalDays)
                    : 0
            };

            vm.PorCategoria = reclamos
                .GroupBy(r => r.Categoria?.Nombre ?? "Sin categoría")
                .Select(g => new MetricaDesgloseVM
                {
                    Nombre = g.Key,
                    Cantidad = g.Count(),
                    PromedioDias = g.Where(r => r.Estado == "Resuelto" && r.FechaActualizacion.HasValue)
                                    .Select(r => (r.FechaActualizacion!.Value - r.FechaCreacion).TotalDays)
                                    .DefaultIfEmpty(0).Average()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            vm.PorDepartamento = reclamos
                .Where(r => r.DepartamentoAsignado != null)
                .GroupBy(r => r.DepartamentoAsignado!.Nombre)
                .Select(g => new MetricaDesgloseVM
                {
                    Nombre = g.Key,
                    Cantidad = g.Count(),
                    PromedioDias = g.Where(r => r.Estado == "Resuelto" && r.FechaActualizacion.HasValue)
                                    .Select(r => (r.FechaActualizacion!.Value - r.FechaCreacion).TotalDays)
                                    .DefaultIfEmpty(0).Average()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            vm.PorBarrio = reclamos
                .GroupBy(r => r.Barrio?.Nombre ?? "Sin barrio")
                .Select(g => new MetricaDesgloseVM
                {
                    Nombre = g.Key,
                    Cantidad = g.Count(),
                    PromedioDias = g.Where(r => r.Estado == "Resuelto" && r.FechaActualizacion.HasValue)
                                    .Select(r => (r.FechaActualizacion!.Value - r.FechaCreacion).TotalDays)
                                    .DefaultIfEmpty(0).Average()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            vm.PorDepartamentoPendientes = reclamos
                .Where(r => r.DepartamentoAsignado != null && r.Estado != "Resuelto")
                .GroupBy(r => r.DepartamentoAsignado!.Nombre)
                .Select(g => new MetricaDesgloseVM { Nombre = g.Key, Cantidad = g.Count(), PromedioDias = 0 })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            vm.EstadoPorBarrio = reclamos
                .GroupBy(r => r.Barrio?.Nombre ?? "Sin barrio")
                .Select(g => new EstadoBarrioVM
                {
                    Barrio = g.Key,
                    Pendiente = g.Count(r => r.Estado == "Pendiente"),
                    Aceptado = g.Count(r => r.Estado == "Aceptado"),
                    EnProceso = g.Count(r => r.Estado == "En Proceso"),
                    Resuelto = g.Count(r => r.Estado == "Resuelto"),
                    Rechazado = g.Count(r => r.Estado == "Rechazado")
                })
                .OrderByDescending(x => x.Pendiente + x.EnProceso)
                .ToList();

            var idsConNotificacion = await _context.Notificaciones
                .Select(n => n.IdUsuario)
                .Distinct()
                .ToListAsync();

            vm.Notificaciones = new NotificacionesResumenVM
            {
                TotalReclamos = reclamos.Count,
                Notificados = reclamos.Count(r => idsConNotificacion.Contains(r.IdUsuario)),
                NoNotificados = reclamos.Count(r => !idsConNotificacion.Contains(r.IdUsuario))
            };

            vm.MotivoPorBarrio = reclamos
                .GroupBy(r => r.Barrio?.Nombre ?? "Sin barrio")
                .Select(g => new MotivoPorBarrioVM
                {
                    Barrio = g.Key,
                    Categorias = g.GroupBy(r => r.Categoria?.Nombre ?? "Sin categoría")
                                  .Select(cg => new CategoriaCantidadVM { Categoria = cg.Key, Cantidad = cg.Count() })
                                  .OrderByDescending(c => c.Cantidad)
                                  .ToList()
                })
                .OrderByDescending(b => b.Categorias.Sum(c => c.Cantidad))
                .ToList();

            return vm;
        }

        public async Task<List<int>> ObtenerAniosDisponibles()
        {
            return await _context.Reclamos
                .Select(r => r.FechaCreacion.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        public async Task<TendenciaVM> ObtenerTendencia(List<string> periodos)
        {
            var vm = new TendenciaVM();
            var nombresMeses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            foreach (var periodo in periodos)
            {
                var partes = periodo.Split('-');
                var anio = int.Parse(partes[0]);

                if (partes.Length == 1)
                {
                    // Año completo: agrupar por mes
                    var reclamosAnio = await _context.Reclamos
                        .Where(r => r.FechaCreacion.Year == anio)
                        .ToListAsync();

                    var datosPorMes = Enumerable.Range(1, 12)
                        .Select(mes => reclamosAnio.Count(r => r.FechaCreacion.Month == mes))
                        .ToList();

                    if (vm.Labels.Count == 0)
                        vm.Labels = nombresMeses.ToList();

                    vm.Series.Add(new SerieTendenciaVM { Nombre = anio.ToString(), Datos = datosPorMes });
                }
                else
                {
                    // Mes específico: agrupar por día
                    var mes = int.Parse(partes[1]);
                    var diasEnMes = DateTime.DaysInMonth(anio, mes);

                    var reclamosMes = await _context.Reclamos
                        .Where(r => r.FechaCreacion.Year == anio && r.FechaCreacion.Month == mes)
                        .ToListAsync();

                    var datosPorDia = Enumerable.Range(1, diasEnMes)
                        .Select(dia => reclamosMes.Count(r => r.FechaCreacion.Day == dia))
                        .ToList();

                    if (vm.Labels.Count < diasEnMes)
                        vm.Labels = Enumerable.Range(1, diasEnMes).Select(d => d.ToString()).ToList();

                    vm.Series.Add(new SerieTendenciaVM { Nombre = $"{nombresMeses[mes - 1]} {anio}", Datos = datosPorDia });
                }
            }

            return vm;
        }

        public async Task<List<Reclamo>> ObtenerReclamosVencidos()
        {
            var limite = DateTime.UtcNow.AddDays(-7);

            return await _context.Reclamos
                .Include(r => r.Usuario)
                .Include(r => r.Categoria)
                .Include(r => r.Subcategoria)
                .Include(r => r.Barrio)
                .Include(r => r.DepartamentoAsignado)
                .Where(r => r.Estado == "Pendiente" && r.FechaCreacion <= limite)
                .OrderBy(r => r.FechaCreacion)
                .ToListAsync();
        }
    }
}