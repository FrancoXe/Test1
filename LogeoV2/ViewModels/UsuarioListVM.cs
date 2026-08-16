using LogeoV2.Models;

namespace LogeoV2.ViewModels
{
    public class UsuariosListVM
    {
        public List<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public List<Rol> RolesDisponibles { get; set; } = new List<Rol>();
        public List<Departamento> DepartamentosDisponibles { get; set; } = new List<Departamento>();
        public string? Busqueda { get; set; }
        public string? OrdenarPor { get; set; }
        public bool Ascendente { get; set; } = true;
    }
}