using System.ComponentModel.DataAnnotations;

namespace LogeoV2.Models
{
    public class Barrio
    {
        public Barrio() { Nombre = string.Empty; }

        public int IdBarrio { get; set; }

        [Required(ErrorMessage = "El nombre del barrio es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}