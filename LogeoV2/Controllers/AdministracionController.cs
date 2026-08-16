using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LogeoV2.Services;
using LogeoV2.Models;
using LogeoV2.ViewModels;

namespace LogeoV2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministracionController : Controller
    {
        private readonly IRolService _rolService;
        private readonly IPermisoService _permisoService;
        private readonly IUsuarioService _usuarioService;
        private readonly IExportService _exportService;

        public AdministracionController(
            IRolService rolService,
            IPermisoService permisoService,
            IUsuarioService usuarioService,
            IExportService exportService)
        {
            _rolService = rolService;
            _permisoService = permisoService;
            _usuarioService = usuarioService;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles = await _rolService.ObtenerTodosLosRoles();
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> DetallesRol(int idRol)
        {
            var rol = await _rolService.ObtenerRolPorId(idRol);
            if (rol == null)
                return NotFound();

            var permisos = await _permisoService.ObtenerPermisosPorRol(rol.NombreRol);
            var viewModel = new RolDetallesVM
            {
                Rol = rol,
                Permisos = permisos.ToList()
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AsignarPermisos(int idRol)
        {
            var rol = await _rolService.ObtenerRolPorId(idRol);
            if (rol == null)
                return NotFound();

            var todosPermisos = await _permisoService.ObtenerTodosLosPermisos();
            var permisosRol = await _permisoService.ObtenerPermisosPorRol(rol.NombreRol);
            var viewModel = new AsignarPermisosVM
            {
                IdRol = idRol,
                NombreRol = rol.NombreRol,
                TodosPermisos = todosPermisos.ToList(),
                PermisosSeleccionados = permisosRol.Select(p => p.Nombre).ToList()
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Usuarios(string? busqueda, string? ordenarPor, bool ascendente = true)
        {
            var usuarios = await _usuarioService.ObtenerUsuarios(busqueda, ordenarPor, ascendente);
            var roles = await _rolService.ObtenerTodosLosRoles();
            var departamentos = await _usuarioService.ObtenerDepartamentos();

            var viewModel = new UsuariosListVM
            {
                Usuarios = usuarios.ToList(),
                RolesDisponibles = roles.ToList(),
                DepartamentosDisponibles = departamentos,
                Busqueda = busqueda,
                OrdenarPor = ordenarPor,
                Ascendente = ascendente
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AsignarPermisos(AsignarPermisosVM model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.NombreRol))
                return View(model);

            var permisosSeleccionados = model.PermisosSeleccionados ?? new List<string>();
            await _permisoService.AsignarPermisosARol(model.NombreRol, permisosSeleccionados.ToArray());

            TempData["Mensaje"] = "Permisos actualizados exitosamente.";
            return RedirectToAction(nameof(DetallesRol), new { idRol = model.IdRol });
        }

        [HttpPost]
        public async Task<IActionResult> CambiarRolUsuario(int idUsuario, int idRol, string? busqueda, string? ordenarPor, bool ascendente = true)
        {
            var exito = await _usuarioService.CambiarRolUsuario(idUsuario, idRol);
            TempData["Mensaje"] = exito
                ? "Rol actualizado exitosamente."
                : "No se pudo actualizar el rol.";

            return RedirectToAction(nameof(Usuarios), new { busqueda, ordenarPor, ascendente });
        }

        [HttpPost]
        public async Task<IActionResult> AsignarDepartamento(int idUsuario, int? idDepartamento, string? busqueda, string? ordenarPor, bool ascendente = true)
        {
            var exito = await _usuarioService.AsignarDepartamentoUsuario(idUsuario, idDepartamento);
            TempData["Mensaje"] = exito ? "Departamento actualizado." : "No se pudo actualizar.";
            return RedirectToAction(nameof(Usuarios), new { busqueda, ordenarPor, ascendente });
        }

        [HttpGet]
        public async Task<IActionResult> ExportarUsuarios(string formato, string? busqueda, string? ordenarPor, bool ascendente = true)
        {
            var usuarios = await _usuarioService.ObtenerUsuarios(busqueda, ordenarPor, ascendente);

            var encabezados = new List<string> { "ID", "Nombre", "Apellido", "Correo", "Último Acceso", "Rol" };
            var filas = usuarios.Select(u => new List<string>
            {
                u.IDUsuario.ToString(),
                u.Nombre,
                u.Apellido,
                u.Correo,
                u.UltimoAcceso?.ToString("g") ?? "Nunca",
                u.Rol.NombreRol
            }).ToList();

            var fecha = DateTime.Now.ToString("yyyyMMdd_HHmm");

            return formato switch
            {
                "excel" => File(_exportService.ExportarExcel("Usuarios", encabezados, filas),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Usuarios_{fecha}.xlsx"),
                "pdf" => File(_exportService.ExportarPdf("Usuarios", encabezados, filas),
                    "application/pdf", $"Usuarios_{fecha}.pdf"),
                "csv" => File(_exportService.ExportarCsv(encabezados, filas),
                    "text/csv", $"Usuarios_{fecha}.csv"),
                _ => BadRequest("Formato no soportado")
            };
        }
        [HttpPost]
        public async Task<IActionResult> CambiarRolUsuarioAjax(int idUsuario, int idRol)
        {
            var exito = await _usuarioService.CambiarRolUsuario(idUsuario, idRol);
            return Json(new { success = exito });
        }

        [HttpPost]
        public async Task<IActionResult> AsignarDepartamentoAjax(int idUsuario, int? idDepartamento)
        {
            var exito = await _usuarioService.AsignarDepartamentoUsuario(idUsuario, idDepartamento);
            return Json(new { success = exito });
        }
    }
}