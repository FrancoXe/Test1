using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogeoV2.Models
{
    public class Notificacion
    {
        public Notificacion()
        {
            Titulo = string.Empty;
            Mensaje = string.Empty;
        }

        public int IdNotificacion { get; set; }

        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        [Required]
        [StringLength(100)]
        public string Titulo { get; set; }

        [Required]
        [StringLength(300)]
        public string Mensaje { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public bool Leida { get; set; } = false;
    }
}