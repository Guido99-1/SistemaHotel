using System.ComponentModel.DataAnnotations;

namespace SistemaHotel.Shared
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 3)]
        public string Clave { get; set; }
    }
}
