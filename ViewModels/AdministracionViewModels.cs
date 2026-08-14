using LogeoV2.Models;
using System.ComponentModel.DataAnnotations;

namespace LogeoV2.ViewModels
{
    public class RolDetallesVM
    {
        public Rol Rol { get; set; }
        public List<Permiso> Permisos { get; set; } = new List<Permiso>();
    }

    public class AsignarPermisosVM
    {
        [Required]
        public int IdRol { get; set; }

        [Required]
        public string? NombreRol { get; set; } = string.Empty;

        public List<Permiso>? TodosPermisos { get; set; } = new List<Permiso>();

        [Required]
        public List<string>? PermisosSeleccionados { get; set; } = new List<string>();
    }
}
