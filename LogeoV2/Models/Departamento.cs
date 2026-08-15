using System.ComponentModel.DataAnnotations;

namespace LogeoV2.Models
{
    public class Departamento
    {
        public Departamento() { Nombre = string.Empty; }

        public int IdDepartamento { get; set; }

        [Required(ErrorMessage = "El nombre del departamento es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}