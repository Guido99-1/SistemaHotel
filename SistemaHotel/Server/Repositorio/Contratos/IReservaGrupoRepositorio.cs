using SistemaHotel.Server.Models;
using SistemaHotel.Shared;

namespace SistemaHotel.Server.Repositorio.Contratos
{
    public interface IReservaGrupoRepositorio
    {
        Task<ReservaMultipleResponseDTO> CrearReservaMultiple(CrearReservaMultipleDTO request);
        Task<ReservaGrupo?> ObtenerGrupo(int idReservaGrupo);
        Task<List<ReservaGrupo>> ObtenerPorCliente(int idCliente);
    }
}
