using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogeoV2.Models
{
    public class HistorialEstado
    {
        public HistorialEstado()
        {
            EstadoAnterior = string.Empty;
            EstadoNuevo = string.Empty;
        }

        public int IdHistorial { get; set; }

        public int IdReclamo { get; set; }
        [ForeignKey("IdReclamo")]
        public Reclamo? Reclamo { get; set; }

        [Required]
        [StringLength(20)]
        public string EstadoAnterior { get; set; }

        [Required]
        [StringLength(20)]
        public string EstadoNuevo { get; set; }

        public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

        public int IdUsuarioQueCambio { get; set; }
        [ForeignKey("IdUsuarioQueCambio")]
        public Usuario? UsuarioQueCambio { get; set; }
    }
}