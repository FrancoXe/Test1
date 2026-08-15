using LogeoV2.Models;
using System.ComponentModel.DataAnnotations;

namespace LogeoV2.ViewModels
{
    public class ReclamoVM
    {
        [Required(ErrorMessage = "El DNI es requerido")]
        [StringLength(20)]
        public string DNI { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccioná una categoría")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "Seleccioná una subcategoría")]
        public int IdSubcategoria { get; set; }

        [Required(ErrorMessage = "Seleccioná un barrio")]
        public int IdBarrio { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        public IFormFile? Archivo { get; set; }

        public List<Categoria> Categorias { get; set; } = new();
        public List<Subcategoria> Subcategorias { get; set; } = new();
        public List<Barrio> Barrios { get; set; } = new();
    }
}