using LogeoV2.Data;
using LogeoV2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogeoV2.Services
{
    public class RolService : IRolService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<RolService> _logger;

        // Roles predeterminados del sistema
        private static readonly string[] RolesPredeterminados = new[]
        {
            "Vecino",     // Rol básico de usuario
            "Administrador", // Rol con permisos administrativos
            "Moderador",  // Rol con permisos intermedios
            "Invitado"    // Rol con permisos limitados
        };

        public RolService(
            AppDBContext context, 
            ILogger<RolService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> InicializarRolesPredeterminados()
        {
            int rolesCreados = 0;

            foreach (var nombreRol in RolesPredeterminados)
            {
                var rolExistente = await _context.Roles
                    .FirstOrDefaultAsync(r => r.NombreRol == nombreRol);

                if (rolExistente == null)
                {
                    var nuevoRol = new Rol { NombreRol = nombreRol };
                    _context.Roles.Add(nuevoRol);
                    rolesCreados++;

                    _logger.LogInformation($"Rol '{nombreRol}' creado.");
                }
            }

            if (rolesCreados > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Se crearon {rolesCreados} roles nuevos.");
            }

            return rolesCreados;
        }

        public async Task<Rol?> ObtenerRolPorNombre(string nombreRol)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.NombreRol == nombreRol);
        }

        public async Task<Rol> CrearRolSiNoExiste(string nombreRol)
        {
            var rol = await ObtenerRolPorNombre(nombreRol);
            
            if (rol == null)
            {
                rol = new Rol { NombreRol = nombreRol };
                _context.Roles.Add(rol);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Rol '{nombreRol}' creado.");
            }

            return rol;
        }

        public async Task<IEnumerable<Rol>> ObtenerTodosLosRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Rol?> ObtenerRolPorId(int idRol)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.IdRol == idRol);
        }
    }
}
