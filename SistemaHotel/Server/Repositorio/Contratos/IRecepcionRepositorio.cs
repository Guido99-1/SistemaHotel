using SistemaHotel.Server.Models;
using SistemaHotel.Shared;
using System.Linq.Expressions;
using static System.Net.WebRequestMethods;

namespace SistemaHotel.Server.Repositorio.Contratos
{
    public interface IRecepcionRepositorio
    {
       
        Task<IQueryable<Recepcion>> Consultar(Expression<Func<Recepcion, bool>> filtro = null);
        Task<Recepcion> Obtener(Expression<Func<Recepcion, bool>> filtro = null);
        Task<Recepcion> Crear(Recepcion entidad);
        Task<bool> Editar(Recepcion entidad);
        //Task<List<Recepcion>> Reporte(string FechaInicio, string FechaFin);
        Task<List<ReporteDTO>> Reporte(string FechaInicio, string FechaFin);
        //Task<bool> Finalizar(int idRecepcion, DateTime fechaSalidaConfirmacion);
        Task<bool> Finalizar(int idRecepcion, DateTime fechaSalidaConfirmacion, decimal costoPenalidad);
        Task<bool> CambiarHabitacion(int idRecepcion, int idNuevaHabitacion);

    }
}
