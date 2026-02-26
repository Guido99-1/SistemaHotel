using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using System.Globalization;
using System.Linq.Expressions;

namespace SistemaHotel.Server.Repositorio.Implementacion
{
    public class HabitacionRepositorio : IHabitacionRepositorio
    {
        private readonly DbhotelBlazorContext _dbContext;

        public HabitacionRepositorio(DbhotelBlazorContext dbContext)
        {
            _dbContext = dbContext;
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

        // ✅ NUEVO: IDs de habitaciones ocupadas/reservadas en un rango
        public async Task<List<int>> OcupacionPorRango(DateTime fechaInicio, DateTime fechaFin)
        {
            var entrada = fechaInicio.Date;
            var salida = fechaFin.Date;

            var hoy = DateTime.Today;

            var reservas = await _dbContext.Reservas
                .Where(r =>
                    r.Estado == true &&
                    entrada < r.FechaSalidaReserva.Value.Date &&
                    salida > r.FechaEntrada.Value.Date
                )
                .Select(r => r.IdHabitacion.Value)
                .ToListAsync();

            var ocupadasHoy = new List<int>();

            if (entrada <= hoy && salida > hoy)
            {
                ocupadasHoy = await _dbContext.Habitacions
                    .Where(h => h.IdEstadoHabitacion == 1)
                    .Select(h => h.IdHabitacion)
                    .ToListAsync();
            }

            return reservas
                .Union(ocupadasHoy)
                .Distinct()
                .ToList();
        }
    }
}