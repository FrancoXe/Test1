using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogeoV2.Data;
using LogeoV2.Models;
using Microsoft.Extensions.Logging;

namespace LogeoV2.Services
{
    public class PermisoService : IPermisoService
    {
        private readonly ILogger<PermisoService> _logger;
        private readonly IRolService _rolService;

        private readonly Dictionary<string, List<string>> _permisosPorRol = new Dictionary<string, List<string>>
        {
            {
                "Vecino", new List<string>
                {
                    PermisosDefecto.VerPerfil,
                    PermisosDefecto.EditarPerfil
                }
            },
            {
                "Moderador", new List<string>
                {
                    PermisosDefecto.VerPerfil,
                    PermisosDefecto.EditarPerfil,
                    PermisosDefecto.VerRegistros
                }
            },
            {
                "Administrador", new List<string>
                {
                    PermisosDefecto.VerPerfil,
                    PermisosDefecto.EditarPerfil,
                    PermisosDefecto.EliminarPerfil,
                    PermisosDefecto.AdministrarUsuarios,
                    PermisosDefecto.AdministrarRoles,
                    PermisosDefecto.VerRegistros,
                    PermisosDefecto.ConfigurarSistema,
                    PermisosDefecto.VerReportes
                }
            }
        };

        public PermisoService(
            ILogger<PermisoService> logger,
            IRolService rolService)
        {
            _logger = logger;
            _rolService = rolService;
        }

        public async Task<int> InicializarPermisosPredeterminados()
        {
            _logger.LogInformation("Inicializando permisos predeterminados...");
            // Los permisos actualmente viven hardcodeados en _permisosPorRol.
            // Si en el futuro se persisten en la tabla Permiso, acá se sembrarían
            // y se devolvería la cantidad insertada.
            var total = _permisosPorRol.Values.SelectMany(p => p).Distinct().Count();
            await Task.CompletedTask;
            return total;
        }

        public Task AsignarPermisosARol(string nombreRol, string[] permisos)
        {
            if (_permisosPorRol.ContainsKey(nombreRol))
            {
                _permisosPorRol[nombreRol] = permisos.ToList();
            }
            else
            {
                _permisosPorRol.Add(nombreRol, permisos.ToList());
                _logger.LogInformation($"Rol {nombreRol} creado con {permisos.Length} permisos");
            }
            return Task.CompletedTask;
        }

        public Task<bool> RolTienePermiso(string nombreRol, string nombrePermiso)
        {
            if (_permisosPorRol.TryGetValue(nombreRol, out var permisos))
            {
                return Task.FromResult(permisos.Contains(nombrePermiso));
            }
            _logger.LogWarning($"No se encontró el rol {nombreRol}");
            return Task.FromResult(false);
        }

        public Task<IEnumerable<Permiso>> ObtenerPermisosPorRol(string nombreRol)
        {
            if (_permisosPorRol.TryGetValue(nombreRol, out var permisos))
            {
                var permisosList = permisos.Select(p => new Permiso { Nombre = p }).ToList();
                return Task.FromResult<IEnumerable<Permiso>>(permisosList);
            }
            _logger.LogWarning($"No se encontró el rol {nombreRol}");
            return Task.FromResult<IEnumerable<Permiso>>(new List<Permiso>());
        }

        public Task<IEnumerable<Permiso>> ObtenerTodosLosPermisos()
        {
            var todosLosPermisos = _permisosPorRol.Values
                .SelectMany(p => p)
                .Distinct()
                .Select(p => new Permiso { Nombre = p })
                .ToList();
            return Task.FromResult<IEnumerable<Permiso>>(todosLosPermisos);
        }
    }
}