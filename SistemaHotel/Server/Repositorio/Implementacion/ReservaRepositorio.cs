using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using System.Globalization;
using System.Linq.Expressions;
using System.Data;


namespace SistemaHotel.Server.Repositorio.Implementacion
{
    public class ReservaRepositorio : IReservaRepositorio
    {
        private readonly DbhotelBlazorContext _dbContext;
        public ReservaRepositorio(DbhotelBlazorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IQueryable<Reserva>> Consultar(Expression<Func<Reserva, bool>> filtro = null)
        {
            IQueryable<Reserva> queryEntidad =
                filtro == null ? _dbContext.Reservas : _dbContext.Reservas.Where(filtro);

            return queryEntidad;
        }

        public async Task<Reserva> Crear(Reserva entidad)
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // Validaciones mínimas
                if (entidad.IdHabitacion == null || entidad.IdHabitacion <= 0)
                    throw new InvalidOperationException("Debe seleccionar una habitación.");

                if (!entidad.FechaEntrada.HasValue || !entidad.FechaSalidaReserva.HasValue)
                    throw new InvalidOperationException("Debe enviar FechaEntrada y FechaSalidaReserva.");

                var entrada = entidad.FechaEntrada.Value.Date;
                var salida = entidad.FechaSalidaReserva.Value.Date;

                // Hotel real (por noche): salida > entrada
                if (salida <= entrada)
                    throw new InvalidOperationException("La fecha de salida debe ser mayor a la fecha de entrada.");

                // ✅ Validar solapamiento (Recepción activa + Reservas)
                var haySolapamiento = await ExisteSolapamiento(entidad.IdHabitacion.Value, entrada, salida);

                if (haySolapamiento)
                    throw new InvalidOperationException(
                        $"Habitación no disponible. Ya está ocupada o reservada entre {entrada:dd/MM/yyyy} y {salida:dd/MM/yyyy}. " +
                        "Elige otras fechas o selecciona otra habitación."
                    );

                // Cliente
                if (entidad.IdClienteNavigation != null && entidad.IdClienteNavigation.IdCliente == 0)
                {
                    var cliente = entidad.IdClienteNavigation;
                    _dbContext.Clientes.Add(cliente);
                    await _dbContext.SaveChangesAsync();

                    entidad.IdCliente = cliente.IdCliente;
                    entidad.IdClienteNavigation = null;
                }
                else if (entidad.IdClienteNavigation != null)
                {
                    entidad.IdCliente = entidad.IdClienteNavigation.IdCliente;
                    entidad.IdClienteNavigation = null;
                }
                else
                {
                    if (entidad.IdCliente == null || entidad.IdCliente <= 0)
                        throw new InvalidOperationException("Debe seleccionar un cliente.");
                }

                entidad.IdHabitacionNavigation = null;

                entidad.Adelanto ??= 0;
                entidad.Estado = true;
                entidad.EstadoReserva = string.IsNullOrWhiteSpace(entidad.EstadoReserva) ? "RESERVADA" : entidad.EstadoReserva;

                _dbContext.Reservas.Add(entidad);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return entidad;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<bool> HabitacionOcupadaHoy(int idHabitacion)
        {
            return await _dbContext.Habitacions
                .AnyAsync(h => h.IdHabitacion == idHabitacion && h.IdEstadoHabitacion == 3);
        }

        public async Task<bool> Editar(Reserva entidad)
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                if (entidad.IdReserva <= 0)
                    throw new InvalidOperationException("IdReserva inválido.");

                if (entidad.IdHabitacion == null || entidad.IdHabitacion <= 0)
                    throw new InvalidOperationException("Debe seleccionar una habitación.");

                if (entidad.IdCliente == null || entidad.IdCliente <= 0)
                    throw new InvalidOperationException("Debe seleccionar un cliente.");

                if (!entidad.FechaEntrada.HasValue || !entidad.FechaSalidaReserva.HasValue)
                    throw new InvalidOperationException("Debe enviar FechaEntrada y FechaSalidaReserva.");

                var entrada = entidad.FechaEntrada.Value.Date;
                var salida = entidad.FechaSalidaReserva.Value.Date;

                if (salida <= entrada)
                    throw new InvalidOperationException("La fecha de salida debe ser mayor a la fecha de entrada.");

                // ✅ Solapamiento excluyendo esta misma reserva
                var haySolapamiento = await ExisteSolapamiento(
                    entidad.IdHabitacion.Value,
                    entrada,
                    salida,
                    excluirIdReserva: entidad.IdReserva
                );

                if (haySolapamiento)
                    throw new InvalidOperationException(
                        $"Habitación no disponible. Ya está ocupada o reservada entre {entrada:dd/MM/yyyy} y {salida:dd/MM/yyyy}. " +
                        "Elige otras fechas o selecciona otra habitación."
                    );

                // No navegar
                entidad.IdHabitacionNavigation = null;
                entidad.IdClienteNavigation = null;

                entidad.Adelanto ??= 0;
                entidad.Estado = true;
                entidad.EstadoReserva = string.IsNullOrWhiteSpace(entidad.EstadoReserva) ? "RESERVADA" : entidad.EstadoReserva;

                _dbContext.Reservas.Update(entidad);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task<Reserva> Obtener(Expression<Func<Reserva, bool>> filtro = null)
        {
            try
            {
                return await _dbContext.Reservas.Where(filtro).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }


        public async Task<List<Reserva>> Reporte(string FechaInicio, string FechaFin)
        {
            DateTime fech_Inicio = DateTime.ParseExact(
                FechaInicio,
                "dd/MM/yyyy",
                new CultureInfo("es-PE"));

            DateTime fech_Fin = DateTime.ParseExact(
                FechaFin,
                "dd/MM/yyyy",
                new CultureInfo("es-PE"));

            List<Reserva> listaResumen = await _dbContext.Reservas
                .Include(p => p.IdClienteNavigation)
                .Include(v => v.IdHabitacionNavigation)
                .Where(r =>
                    r.FechaEntrada.Value.Date >= fech_Inicio.Date &&
                    r.FechaEntrada.Value.Date <= fech_Fin.Date)
                .ToListAsync();

            return listaResumen;
        }
        public async Task<bool> ExisteCruceReserva(int idHabitacion, DateTime fechaEntrada, DateTime fechaSalida, int? excluirIdReserva = null)
        {
            var x = fechaEntrada.Date; // Entrada nueva
            var y = fechaSalida.Date;  // Salida nueva

            var query = _dbContext.Reservas.Where(r =>
                r.IdHabitacion == idHabitacion &&
                r.Estado == true &&
                r.EstadoReserva != "CANCELADA" &&
                r.FechaEntrada.HasValue &&
                r.FechaSalidaReserva.HasValue
            );

            if (excluirIdReserva.HasValue)
                query = query.Where(r => r.IdReserva != excluirIdReserva.Value);

            // ✅ Hotel real (rango [Entrada, Salida)):
            // Se cruzan si: X < B  &&  Y > A
            return await query.AnyAsync(r =>
                x < r.FechaSalidaReserva.Value.Date &&
                y > r.FechaEntrada.Value.Date
            );
        }
        private async Task<bool> ExisteSolapamiento(
            int idHabitacion,
            DateTime fechaEntrada,
            DateTime fechaSalida,
            int? excluirIdReserva = null)
            {
            var entrada = fechaEntrada.Date;
            var salida = fechaSalida.Date;

            // 1) Validar cruces con RECEPCION ACTIVA (check-in)
            // Estado=true significa recepción activa (ocupando habitación)
            var hayRecepcionActivaCruce = await _dbContext.Recepcions.AnyAsync(r =>
                r.IdHabitacion == idHabitacion
                && r.Estado == true
                && r.FechaEntrada.HasValue
                && r.FechaSalida.HasValue
                && entrada < r.FechaSalida.Value.Date
                && salida > r.FechaEntrada.Value.Date
            );

            if (hayRecepcionActivaCruce)
                return true;

            // 2) Validar cruces con RESERVAS activas
            var hayReservaCruce = await _dbContext.Reservas.AnyAsync(r =>
                r.IdHabitacion == idHabitacion
                && r.Estado == true
                && (!excluirIdReserva.HasValue || r.IdReserva != excluirIdReserva.Value)
                && r.FechaEntrada.HasValue
                && r.FechaSalidaReserva.HasValue
                && entrada < r.FechaSalidaReserva.Value.Date
                && salida > r.FechaEntrada.Value.Date
            );

            return hayReservaCruce;
        }



    }
}
