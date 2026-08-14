using LogeoV2.Data;
using LogeoV2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogeoV2.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(AppDBContext context, ILogger<UsuarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Usuario>> ObtenerUsuarios(string? busqueda, string? ordenarPor, bool ascendente)
        {
            var query = _context.Usuarios
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var termino = busqueda.Trim().ToLower();
                query = query.Where(u =>
                    u.Nombre.ToLower().Contains(termino) ||
                    u.Apellido.ToLower().Contains(termino) ||
                    u.Correo.ToLower().Contains(termino));
            }

            query = ordenarPor switch
            {
                "id" => ascendente ? query.OrderBy(u => u.IDUsuario) : query.OrderByDescending(u => u.IDUsuario),
                "rol" => ascendente ? query.OrderBy(u => u.Rol.NombreRol) : query.OrderByDescending(u => u.Rol.NombreRol),
                "nombre" => ascendente ? query.OrderBy(u => u.Nombre) : query.OrderByDescending(u => u.Nombre),
                _ => query.OrderBy(u => u.IDUsuario)
            };

            return await query.ToListAsync();
        }

        public async Task<bool> CambiarRolUsuario(int idUsuario, int idRol)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null)
            {
                _logger.LogWarning($"Usuario {idUsuario} no encontrado al intentar cambiar rol");
                return false;
            }

            var rolExiste = await _context.Roles.AnyAsync(r => r.IdRol == idRol);
            if (!rolExiste)
            {
                _logger.LogWarning($"Rol {idRol} no encontrado al intentar asignarlo al usuario {idUsuario}");
                return false;
            }

            usuario.IdRol = idRol;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Rol del usuario {idUsuario} actualizado a {idRol}");
            return true;
        }
    }
}