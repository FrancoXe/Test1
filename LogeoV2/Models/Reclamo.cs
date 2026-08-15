using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogeoV2.Models
{
    public class Reclamo
    {
        public Reclamo()
        {
            DNI = string.Empty;
            Direccion = string.Empty;
            Descripcion = string.Empty;
            Estado = "Pendiente";
        }

        public int IdReclamo { get; set; }

        [Required(ErrorMessage = "El DNI es requerido")]
        [StringLength(20)]
        public string DNI { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int IdCategoria { get; set; }
        [ForeignKey("IdCategoria")]
        public Categoria? Categoria { get; set; }

        [Required(ErrorMessage = "La subcategoría es requerida")]
        public int IdSubcategoria { get; set; }
        [ForeignKey("IdSubcategoria")]
        public Subcategoria? Subcategoria { get; set; }

        [Required(ErrorMessage = "El barrio es requerido")]
        public int IdBarrio { get; set; }
        [ForeignKey("IdBarrio")]
        public Barrio? Barrio { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(200)]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(500)]
        public string Descripcion { get; set; }

        [StringLength(200)]
        public string? RutaArchivo { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; }

        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        public int? IdDepartamentoAsignado { get; set; }
        [ForeignKey("IdDepartamentoAsignado")]
        public Departamento? DepartamentoAsignado { get; set; }
    }
}