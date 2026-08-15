using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogeoV2.Models
{
    public class Subcategoria
    {
        public Subcategoria() { Nombre = string.Empty; }

        public int IdSubcategoria { get; set; }

        [Required(ErrorMessage = "El nombre de la subcategoría es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; }

        public int IdCategoria { get; set; }

        [ForeignKey("IdCategoria")]
        public Categoria? Categoria { get; set; }
    }
}