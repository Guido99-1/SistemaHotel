using SistemaHotel.Server.Models;
using System.Linq.Expressions;

namespace SistemaHotel.Server.Repositorio.Contratos
{
    public interface IReservaRepositorio
    {
        Task<IQueryable<Reserva>> Consultar(Expression<Func<Reserva, bool>> filtro = null);

        Task<Reserva> Obtener(Expression<Func<Reserva, bool>> filtro = null);

        Task<Reserva> Crear(Reserva entidad);

        //Task<bool> Editar(Reserva entidad);
        Task<bool> EditarReserva(Reserva entidad);
        Task<bool> Eliminar(int idReserva);
        Task<List<Reserva>> Reporte(string FechaInicio, string FechaFin);
        Task<bool> ExisteCruceReserva(
            int idHabitacion,
            DateTime fechaEntrada,
            DateTime fechaSalida,
            int? excluirIdReserva = null
            );

    }
}
