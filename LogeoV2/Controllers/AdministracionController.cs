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

        public AdministracionController(
            IRolService rolService,
            IPermisoService permisoService,
            IUsuarioService usuarioService)
        {
            _rolService = rolService;
            _permisoService = permisoService;
            _usuarioService = usuarioService;
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

            var viewModel = new UsuariosListVM
            {
                Usuarios = usuarios.ToList(),
                RolesDisponibles = roles.ToList(),
                Busqueda = busqueda,
                OrdenarPor = ordenarPor,
                Ascendente = ascendente
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AsignarPermisos(AsignarPermisosVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _permisoService.AsignarPermisosARol(model.NombreRol, model.PermisosSeleccionados.ToArray());

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
    }
}
