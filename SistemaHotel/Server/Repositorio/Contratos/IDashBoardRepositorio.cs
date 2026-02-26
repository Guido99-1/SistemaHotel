namespace SistemaHotel.Server.Repositorio.Contratos
{
    public interface IDashBoardRepositorio
    {
        Task<int> TotalHabitaciones();
        Task<int> HabitacionesDisponibles();
        Task<int> HabitacionesOcupadas();
        Task<int> HabitacionesLimpieza();
        Task<int> TotalReservasHoy();
        Task<int> TotalReservasMes();
    }
}
