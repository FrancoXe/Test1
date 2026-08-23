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
        private readonly IAuditoriaService _auditoriaService;

        public UsuarioService(AppDBContext context, ILogger<UsuarioService> logger, IAuditoriaService auditoriaService)
        {
            _context = context;
            _logger = logger;
            _auditoriaService = auditoriaService;
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

        public async Task<bool> CambiarRolUsuario(int idUsuario, int idRol, int idUsuarioQueRealiza)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.IDUsuario == idUsuario);
            if (usuario == null)
                return false;

            var rolNuevo = await _context.Roles.FindAsync(idRol);
            if (rolNuevo == null)
                return false;

            var rolAnteriorNombre = usuario.Rol?.NombreRol ?? "Ninguno";

            usuario.IdRol = idRol;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rol del usuario {idUsuario} actualizado a {idRol}");
            await _auditoriaService.RegistrarAccion("Cambio de Rol", "Usuario", idUsuario, rolAnteriorNombre, rolNuevo.NombreRol, idUsuarioQueRealiza);

            return true;
        }
        public async Task<bool> AsignarDepartamentoUsuario(int idUsuario, int? idDepartamento, int idUsuarioQueRealiza)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null)
                return false;

            var deptoAnteriorId = usuario.IdDepartamento;
            var deptoAnteriorNombre = deptoAnteriorId.HasValue
                ? (await _context.Departamentos.FindAsync(deptoAnteriorId.Value))?.Nombre ?? "Desconocido"
                : "Sin asignar";

            var deptoNuevoNombre = idDepartamento.HasValue
                ? (await _context.Departamentos.FindAsync(idDepartamento.Value))?.Nombre ?? "Desconocido"
                : "Sin asignar";

            usuario.IdDepartamento = idDepartamento;
            await _context.SaveChangesAsync();

            await _auditoriaService.RegistrarAccion("Asignación de Departamento", "Usuario", idUsuario, deptoAnteriorNombre, deptoNuevoNombre, idUsuarioQueRealiza);

            return true;
        }

        public async Task<List<Departamento>> ObtenerDepartamentos()
        {
            return await _context.Departamentos.ToListAsync();
        }
    }
}