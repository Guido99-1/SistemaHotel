using SistemaHotel.Shared;

namespace SistemaHotel.Client.Servicios.Contratos
{
    public interface IDashBoardServicio
    {
        Task<ResponseDTO<DashBoardDTO>> Resumen();
        Task<int> TotalReservasHoy();
        Task<int> TotalReservasMes();
        Task<int> TotalHabitacionesDisponibles();
        Task<int> TotalHabitacionesOcupadas();
        Task<int> TotalHabitacionesEnLimpieza();
        Task<int> TotalHabitaciones();

    }
}
