namespace SistemaHotel.Shared
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
        public UsuarioDTO Usuario { get; set; }
    }
}
