using SistemaHotel.Shared;

namespace SistemaHotel.Client.Servicios.Contratos
{
    public interface IReservaServicio
    {
        Task<ResponseDTO<ReservaDTO>> Obtener(int idHabitacion);
        Task<ResponseDTO<ReservaDTO>> Crear(ReservaDTO entidad);
        //Task<bool> Editar(ReservaDTO entidad);

        Task<ResponseDTO<List<ReservaDTO>>> Reporte(string fechaInicio, string fechaFin);
        Task<ResponseDTO<List<ReservaDTO>>> Filtrar(string fechaInicio, string fechaFin);

        // ✅ Para listado (tabla)
        Task<ResponseDTO<List<ReservaReporteDTO>>> FiltrarListado(string fechaInicio, string fechaFin);

        // ✅ Acciones sobre reserva
        Task<ResponseDTO<bool>> CambiarEstado(int idReserva, string estadoReserva);
        Task<ResponseDTO<bool>> Cancelar(int idReserva);
        Task<ResponseDTO<ReservaDTO>> ObtenerPorId(int idReserva);
        Task<ResponseDTO<ReservaDTO>> EditarReserva(int idReserva, ReservaDTO request);
        Task<ResponseDTO<bool>> Eliminar(int idReserva);

        // ✅ PDF
        Task<byte[]> ExportarPdf(string fechaInicio, string fechaFin);
    }
}