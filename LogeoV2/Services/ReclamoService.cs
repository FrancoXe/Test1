using LogeoV2.Data;
using LogeoV2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogeoV2.Services
{
    public class ReclamoService : IReclamoService
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ReclamoService> _logger;

        private static readonly string[] EstadosValidos = { "Pendiente", "Aceptado", "Rechazado", "En Proceso", "Resuelto" };

        public ReclamoService(AppDBContext context, IWebHostEnvironment webHostEnvironment, ILogger<ReclamoService> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
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

        public async Task<bool> CambiarEstadoReclamo(int idReclamo, string nuevoEstado)
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
    }
}