using System.ComponentModel.DataAnnotations;

namespace SistemaHotel.Shared
{
    public class CambiarPasswordDTO
    {
        [Required(ErrorMessage = "El IdUsuario es requerido")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NuevaClave { get; set; } = string.Empty;
    }
}
