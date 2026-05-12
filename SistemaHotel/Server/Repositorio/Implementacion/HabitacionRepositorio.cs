using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Server.Utilidades;
using System.Globalization;
using System.Linq.Expressions;

namespace SistemaHotel.Server.Repositorio.Implementacion
{
    public class HabitacionRepositorio : IHabitacionRepositorio
    {
        private readonly DbhotelBlazorContext _dbContext;
        private readonly IFechaService _fechaService;

        public HabitacionRepositorio(DbhotelBlazorContext dbContext, IFechaService fechaService)
        {
            _dbContext = dbContext;
            _fechaService = fechaService;
        }

        public async Task<IQueryable<Habitacion>> Consultar(Expression<Func<Habitacion, bool>> filtro = null)
        {
            IQueryable<Habitacion> queryEntidad =
                filtro == null ? _dbContext.Habitacions : _dbContext.Habitacions.Where(filtro);

            return queryEntidad;
        }

        public async Task<Habitacion> Crear(Habitacion entidad)
        {
            try
            {
                _dbContext.Set<Habitacion>().Add(entidad);
                await _dbContext.SaveChangesAsync();
                return entidad;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(Habitacion entidad)
        {
            try
            {
                _dbContext.Habitacions.Update(entidad);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(Habitacion entidad)
        {
            try
            {
                _dbContext.Habitacions.Remove(entidad);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Habitacion>> Lista()
        {
            try
            {
                return await _dbContext.Habitacions.ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<Habitacion> Obtener(Expression<Func<Habitacion, bool>> filtro = null)
        {
            try
            {
                return await _dbContext.Habitacions.Where(filtro).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Devuelve los IDs de habitaciones que están ocupadas o reservadas
        /// en el rango de fechas indicado.
        ///
        /// REGLA DE NEGOCIO DEL HOTEL:
        /// - Check-in: 1pm
        /// - Check-out: 12pm (mediodía) del día de salida
        /// - La habitación se libera el mismo día de salida (en la mañana)
        /// - Por lo tanto, una habitación con salida el día X NO bloquea
        ///   una nueva reserva con entrada el mismo día X
        ///
        /// Lógica de cruce: hay cruce si entrada < salidaExistente AND salida > entradaExistente
        /// (NO incluye fechas adyacentes como cruce)
        /// </summary>
        public async Task<List<int>> OcupacionPorRango(DateTime fechaInicio, DateTime fechaFin)
        {
            var entrada = fechaInicio.Date;
            var salida = fechaFin.Date;

            // ✅ 1. Reservas activas que cruzan con el rango solicitado
            // Solo bloquean: RESERVADA, CONFIRMADA, CHECKIN
            // NO bloquean: CANCELADA, FINALIZADA
            var estadosBloqueantes = new[] { "RESERVADA", "CONFIRMADA", "CHECKIN" };

            var habitacionesReservadas = await _dbContext.Reservas
                .Where(r =>
                    r.Estado == true &&
                    estadosBloqueantes.Contains(r.EstadoReserva) &&
                    r.IdHabitacion.HasValue &&
                    r.FechaEntrada.HasValue &&
                    r.FechaSalidaReserva.HasValue &&
                    entrada < r.FechaSalidaReserva.Value.Date &&
                    salida > r.FechaEntrada.Value.Date
                )
                .Select(r => r.IdHabitacion!.Value)
                .ToListAsync();

            // ✅ 2. Recepciones (check-ins) activas que cruzan con el rango
            var habitacionesEnRecepcion = await _dbContext.Recepcions
                .Where(r =>
                    r.Estado == true &&
                    r.IdHabitacion.HasValue &&
                    r.FechaEntrada.HasValue &&
                    r.FechaSalida.HasValue &&
                    entrada < r.FechaSalida.Value.Date &&
                    salida > r.FechaEntrada.Value.Date
                )
                .Select(r => r.IdHabitacion!.Value)
                .ToListAsync();

            return habitacionesReservadas
                .Union(habitacionesEnRecepcion)
                .Distinct()
                .ToList();
        }
    }
}