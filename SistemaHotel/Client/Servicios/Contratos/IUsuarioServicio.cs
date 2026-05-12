using SistemaHotel.Shared;

namespace SistemaHotel.Client.Servicios.Contratos
{
    public interface IUsuarioServicio
    {
        Task<ResponseDTO<List<UsuarioDTO>>> Lista();
        Task<ResponseDTO<LoginResponseDTO>> IniciarSesion(LoginRequestDTO loginRequest);
        Task<ResponseDTO<UsuarioDTO>> Crear(UsuarioDTO entidad);
        Task<bool> Editar(UsuarioDTO entidad);
        Task<bool> Eliminar(int id);
        Task<ResponseDTO<string>> CambiarPassword(CambiarPasswordDTO request);
    }
}
