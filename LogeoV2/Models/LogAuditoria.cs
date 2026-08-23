using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogeoV2.Models
{
    public class LogAuditoria
    {
        public LogAuditoria()
        {
            Accion = string.Empty;
            Entidad = string.Empty;
        }

        public int IdLog { get; set; }

        [Required]
        [StringLength(50)]
        public string Accion { get; set; }

        [Required]
        [StringLength(50)]
        public string Entidad { get; set; }

        public int IdEntidad { get; set; }

        [StringLength(200)]
        public string? DetalleAnterior { get; set; }

        [StringLength(200)]
        public string? DetalleNuevo { get; set; }

        public DateTime FechaAccion { get; set; } = DateTime.UtcNow;

        public int IdUsuarioQueRealizo { get; set; }
        [ForeignKey("IdUsuarioQueRealizo")]
        public Usuario? UsuarioQueRealizo { get; set; }
    }
}