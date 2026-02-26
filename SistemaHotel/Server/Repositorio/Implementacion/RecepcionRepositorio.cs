using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Shared;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace SistemaHotel.Server.Repositorio.Implementacion
{
    public class RecepcionRepositorio : IRecepcionRepositorio
    {

        private readonly DbhotelBlazorContext _dbContext;

        public RecepcionRepositorio(DbhotelBlazorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IQueryable<Recepcion>> Consultar(Expression<Func<Recepcion, bool>> filtro = null)
        {
            IQueryable<Recepcion> queryEntidad = filtro == null ? _dbContext.Recepcions : _dbContext.Recepcions.Where(filtro);
            return queryEntidad;
        }

        public async Task<Recepcion> Crear(Recepcion entidad)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // ✅ Validaciones mínimas
                if (entidad.IdHabitacion == null)
                    throw new Exception("Debe enviar la habitación.");

                if (!entidad.FechaEntrada.HasValue || !entidad.FechaSalida.HasValue)
                    throw new Exception("Debe enviar FechaEntrada y FechaSalida.");

                // OJO: en hotel real se permite Entrada == Salida? NO (0 noches no tiene sentido)
                // Para 1 noche: Entrada 19, Salida 20  -> OK
                if (entidad.FechaSalida.Value.Date <= entidad.FechaEntrada.Value.Date)
                    throw new Exception("La fecha de salida debe ser mayor a la fecha de entrada.");

                // -----------------------------
                // Cliente
                // -----------------------------
                if (entidad.IdClienteNavigation == null)
                    throw new Exception("Debe enviar el cliente.");

                if (entidad.IdClienteNavigation.IdCliente == 0)
                {
                    var cliente = entidad.IdClienteNavigation;
                    _dbContext.Clientes.Add(cliente);
                    await _dbContext.SaveChangesAsync();

                    entidad.IdCliente = cliente.IdCliente;
                    entidad.IdClienteNavigation = null;
                }
                else
                {
                    entidad.IdCliente = entidad.IdClienteNavigation.IdCliente;
                    entidad.IdClienteNavigation = null;
                }

                // -----------------------------
                // ✅ Actualizar reserva a EN_ESTADIA (si existe una que cruza)
                // -----------------------------
                var entrada = entidad.FechaEntrada.Value.Date;
                var salida = entidad.FechaSalida.Value.Date;

                var reserva = await _dbContext.Reservas
                    .Where(r =>
                        r.Estado == true &&
                        r.IdHabitacion == entidad.IdHabitacion &&
                        // Solo reservas "activas" (ajusta a tus nombres reales)
                        (r.EstadoReserva == "CONFIRMADA") &&
                        r.FechaEntrada.HasValue &&
                        r.FechaSalidaReserva.HasValue &&
                        // ✅ Regla de cruce que permite salida==entrada
                        entrada < r.FechaSalidaReserva.Value.Date &&
                        salida > r.FechaEntrada.Value.Date
                    )
                    .OrderBy(r => r.FechaEntrada)
                    .FirstOrDefaultAsync();

                if (reserva != null)
                {
                    reserva.EstadoReserva = "EN_ESTADIA";
                    _dbContext.Reservas.Update(reserva);
                    // NO guardo aún, lo hacemos al final
                }

                // -----------------------------
                // Habitación -> Ocupado
                // -----------------------------
                var habitacion = await _dbContext.Habitacions
                    .FirstAsync(h => h.IdHabitacion == entidad.IdHabitacion);

                habitacion.IdEstadoHabitacion = 3; // Ocupado
                _dbContext.Habitacions.Update(habitacion);

                // -----------------------------
                // Recepción
                // -----------------------------
                if (entidad.Adelanto == null)
                    entidad.Adelanto = 0;

                _dbContext.Recepcions.Add(entidad);

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
        public async Task<bool> Finalizar(int idRecepcion, DateTime fechaSalidaConfirmacion)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1) Traer recepción activa
                var recepcion = await _dbContext.Recepcions
                    .FirstOrDefaultAsync(r => r.IdRecepcion == idRecepcion);

                if (recepcion == null)
                    throw new Exception("No se encontró la recepción.");

                if (recepcion.Estado == false)
                    throw new Exception("Esta recepción ya fue finalizada.");

                // 2) Cerrar recepción
                recepcion.FechaSalidaConfirmacion = fechaSalidaConfirmacion;
                recepcion.Estado = false;

                _dbContext.Recepcions.Update(recepcion);
                await _dbContext.SaveChangesAsync();

                // 3) Pasar habitación a LIMPIEZA (recomendado)
                var habitacion = await _dbContext.Habitacions
                    .FirstOrDefaultAsync(h => h.IdHabitacion == recepcion.IdHabitacion);

                if (habitacion == null)
                    throw new Exception("No se encontró la habitación asociada a la recepción.");

                // 2 = Limpieza (según tu tabla EstadoHabitacion)
                habitacion.IdEstadoHabitacion = 2;
                _dbContext.Habitacions.Update(habitacion);
                await _dbContext.SaveChangesAsync();

                // 4) Marcar la reserva correspondiente como FINALIZADA
                //    Regla de cruce permitiendo salida==entrada (hotel real):
                //    hay cruce real si: (start < otherEnd) AND (end > otherStart)
                var entrada = recepcion.FechaEntrada ?? DateTime.MinValue;
                var salida = recepcion.FechaSalida ?? fechaSalidaConfirmacion;

                var reserva = await _dbContext.Reservas
                    .Where(r =>
                        r.Estado == true &&
                        r.IdHabitacion == recepcion.IdHabitacion &&
                        r.FechaEntrada.HasValue &&
                        r.FechaSalidaReserva.HasValue &&
                        // cruce real (permite salida==entrada):
                        r.FechaEntrada.Value.Date < salida.Date &&
                        r.FechaSalidaReserva.Value.Date > entrada.Date &&
                        // reservas que aún no han finalizado:
                        r.EstadoReserva != "FINALIZADA"
                    )
                    // la más reciente suele ser la correcta
                    .OrderByDescending(r => r.IdReserva)
                    .FirstOrDefaultAsync();

                if (reserva != null)
                {
                    reserva.EstadoReserva = "FINALIZADA";

                    // ✅ recomendado para que NO se considere activa:
                    // (tu código ya consulta reservas activas con Estado==true)
                    reserva.Estado = false;

                    _dbContext.Reservas.Update(reserva);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task<bool> Editar(Recepcion entidad)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var habitacion = await _dbContext.Habitacions.Where(h => h.IdHabitacion == entidad.IdHabitacion).FirstAsync();

                    habitacion.IdEstadoHabitacion = 2;
                    _dbContext.Habitacions.Update(habitacion);
                    await _dbContext.SaveChangesAsync();

                    entidad.Estado = false;
                    entidad.IdHabitacionNavigation = null;
                    entidad.IdClienteNavigation = null;
                    _dbContext.Recepcions.Update(entidad);
                    await _dbContext.SaveChangesAsync();

                    transaction.Commit();

                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }



        public async Task<Recepcion> Obtener(Expression<Func<Recepcion, bool>> filtro = null)
        {
            try
            {
                return await _dbContext.Recepcions.Where(filtro).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        public  async Task<List<Recepcion>> Reporte(string FechaInicio, string FechaFin)
        {
            DateTime fech_Inicio = DateTime.ParseExact(FechaInicio, "dd/MM/yyyy", new CultureInfo("es-PE"));
            DateTime fech_Fin = DateTime.ParseExact(FechaFin, "dd/MM/yyyy", new CultureInfo("es-PE"));

            List<Recepcion> listaResumen = await _dbContext.Recepcions
                .Include(p => p.IdClienteNavigation)
                .Include(v => v.IdHabitacionNavigation)
                 .Where(dv => dv.FechaEntrada.Value.Date >= fech_Inicio.Date && dv.FechaEntrada.Value.Date <= fech_Fin.Date)
                .ToListAsync();

            return listaResumen;
        }
    }
}
