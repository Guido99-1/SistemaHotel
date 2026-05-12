using SistemaHotel.Shared;

namespace SistemaHotel.Client.Servicios.Contratos
{
    public interface IReservaGrupoServicio
    {
        Task<ResponseDTO<ReservaMultipleResponseDTO>> CrearReservaMultiple(CrearReservaMultipleDTO request);
        Task<ResponseDTO<ReservaGrupoDTO>> ObtenerGrupo(int idReservaGrupo);
        Task<ResponseDTO<List<ReservaGrupoDTO>>> ObtenerPorCliente(int idCliente);
    }
}
