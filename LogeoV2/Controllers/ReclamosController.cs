using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogeoV2.Data;
using LogeoV2.Models;
using LogeoV2.Services;
using LogeoV2.ViewModels;


namespace LogeoV2.Controllers
{
    [Authorize]
    public class ReclamosController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IReclamoService _reclamoService;
        private readonly IExportService _exportService;

        public ReclamosController(AppDBContext context, IReclamoService reclamoService, IExportService exportService)
        {
            _context = context;
            _reclamoService = reclamoService;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<IActionResult> NuevoModal()
        {
            var model = new ReclamoVM
            {
                Categorias = await _reclamoService.ObtenerCategorias(),
                Barrios = await _reclamoService.ObtenerBarrios()
            };
            return PartialView("_NuevoReclamoModal", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubcategorias(int idCategoria)
        {
            var subcategorias = await _reclamoService.ObtenerSubcategorias(idCategoria);
            return Json(subcategorias.Select(s => new { id = s.IdSubcategoria, nombre = s.Nombre }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ReclamoVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categorias = await _reclamoService.ObtenerCategorias();
                model.Barrios = await _reclamoService.ObtenerBarrios();
                if (model.IdCategoria > 0)
                    model.Subcategorias = await _reclamoService.ObtenerSubcategorias(model.IdCategoria);

                return PartialView("_NuevoReclamoModal", model);
            }

            var correo = User.FindFirst("Correo")?.Value;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario == null)
                return Unauthorized();

            var reclamo = new Reclamo
            {
                DNI = model.DNI,
                IdCategoria = model.IdCategoria,
                IdSubcategoria = model.IdSubcategoria,
                IdBarrio = model.IdBarrio,
                Direccion = model.Direccion,
                Descripcion = model.Descripcion,
                IdUsuario = usuario.IDUsuario
            };

            await _reclamoService.CrearReclamo(reclamo, model.Archivo);

            return Json(new { success = true, mensaje = "Reclamo creado exitosamente" });
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Gestionar(string? estado)
        {
            var reclamos = await _reclamoService.ObtenerReclamos(estado);
            var viewModel = new GestionReclamosVM
            {
                Reclamos = reclamos,
                Estado = estado
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int idReclamo, string nuevoEstado, string? estado)
        {
            var exito = await _reclamoService.CambiarEstadoReclamo(idReclamo, nuevoEstado);
            TempData["Mensaje"] = exito ? "Estado actualizado exitosamente." : "No se pudo actualizar el estado.";
            return RedirectToAction(nameof(Gestionar), new { estado });
        }
        [HttpGet]
        public async Task<IActionResult> MisReclamos()
        {
            var correo = User.FindFirst("Correo")?.Value;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario == null)
                return Unauthorized();

            var reclamos = await _context.Reclamos
                .Include(r => r.Categoria)
                .Include(r => r.Subcategoria)
                .Include(r => r.Barrio)
                .Where(r => r.IdUsuario == usuario.IDUsuario)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return View(reclamos);
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Historial(string? estado, int? idCategoria, int? idBarrio, DateTime? fechaDesde, DateTime? fechaHasta, string? busqueda)
        {
            var reclamos = await _reclamoService.ObtenerHistorial(estado, idCategoria, idBarrio, fechaDesde, fechaHasta, busqueda);

            var viewModel = new HistorialReclamosVM
            {
                Reclamos = reclamos,
                Categorias = await _reclamoService.ObtenerCategorias(),
                Barrios = await _reclamoService.ObtenerBarrios(),
                Estado = estado,
                IdCategoria = idCategoria,
                IdBarrio = idBarrio,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Busqueda = busqueda
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ExportarHistorial(string formato, string? estado, int? idCategoria, int? idBarrio, DateTime? fechaDesde, DateTime? fechaHasta, string? busqueda)
        {
            var reclamos = await _reclamoService.ObtenerHistorial(estado, idCategoria, idBarrio, fechaDesde, fechaHasta, busqueda);

            var encabezados = new List<string> { "ID", "DNI", "Vecino", "Categoría", "Subcategoría", "Barrio", "Dirección", "Descripción", "Fecha", "Estado" };
            var filas = reclamos.Select(r => new List<string>
    {
        r.IdReclamo.ToString(),
        r.DNI,
        $"{r.Usuario?.Nombre} {r.Usuario?.Apellido}",
        r.Categoria?.Nombre ?? "",
        r.Subcategoria?.Nombre ?? "",
        r.Barrio?.Nombre ?? "",
        r.Direccion,
        r.Descripcion,
        r.FechaCreacion.ToString("g"),
        r.Estado
    }).ToList();

            var fecha = DateTime.Now.ToString("yyyyMMdd_HHmm");

            return formato switch
            {
                "excel" => File(_exportService.ExportarExcel("Historial de Reclamos", encabezados, filas),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Historial_{fecha}.xlsx"),
                "pdf" => File(_exportService.ExportarPdf("Historial de Reclamos", encabezados, filas),
                    "application/pdf", $"Historial_{fecha}.pdf"),
                "csv" => File(_exportService.ExportarCsv(encabezados, filas),
                    "text/csv", $"Historial_{fecha}.csv"),
                _ => BadRequest("Formato no soportado")
            };
        }
    }
}