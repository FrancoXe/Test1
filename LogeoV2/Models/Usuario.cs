using System.ComponentModel.DataAnnotations;

namespace LogeoV2.Models
{
    public class Usuario
    {
        public Usuario()
        {
            Nombre = string.Empty;
            Apellido = string.Empty;
            Correo = string.Empty;
            Clave = string.Empty;
            Rol = new Rol();
        }

        public int IDUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La clave es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe más de 6 caracteres")]
        public string Clave { get; set; }

        public int IdRol { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        public Rol Rol { get; set; }

        public DateTime? UltimoAcceso { get; set; }
    }
}
