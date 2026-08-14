using System.ComponentModel.DataAnnotations;

namespace LogeoV2.ViewModels
{
    public class LoginVM
    {
        public LoginVM()
        {
            Correo = string.Empty;
            Clave = string.Empty;
        }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede superar los 100 caracteres")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener más de 6 caracteres")]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string Clave { get; set; }

        [Display(Name = "Recordarme")]
        public bool RecordarMe { get; set; }
    }
}
