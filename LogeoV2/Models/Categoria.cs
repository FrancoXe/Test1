using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogeoV2.Models
{
    public class Categoria
    {
        public Categoria() { Nombre = string.Empty; }

        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; }

        public int? IdDepartamento { get; set; }

        [ForeignKey("IdDepartamento")]
        public Departamento? Departamento { get; set; }
    }
}