using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Shared;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using static System.Net.WebRequestMethods;

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
            // ✅ Aislamiento Serializable previene race conditions de check-in simultáneo
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // ✅ Validaciones mínimas
                if (entidad.IdHabitacion == null)
                    throw new Exception("Debe enviar la habitación.");

                if (entidad.FechaEntrada.HasValue)
                    entidad.FechaEntrada = DateTime.SpecifyKind(entidad.FechaEntrada.Value.Date, DateTimeKind.Unspecified);

                if (entidad.FechaSalida.HasValue)
                    entidad.FechaSalida = DateTime.SpecifyKind(entidad.FechaSalida.Value.Date, DateTimeKind.Unspecified);

                if (!entidad.FechaEntrada.HasValue || !entidad.FechaSalida.HasValue)
                    throw new Exception("Debe enviar FechaEntrada y FechaSalida.");

                // OJO: en hotel real se permite Entrada == Salida? NO (0 noches no tiene sentido)
                // Para 1 noche: Entrada 19, Salida 20  -> OK
                if (entidad.FechaSalida.Value.Date <= entidad.FechaEntrada.Value.Date)
                    throw new Exception("La fecha de salida debe ser mayor a la fecha de entrada.");

                // ✅ FIX #6: validar fechas pasadas (defensa en backend, no confiar solo en UI)
                if (entidad.FechaEntrada.Value.Date < DateTime.Today)
                    throw new Exception("La fecha de entrada no puede ser anterior a hoy.");

                // 🔴 ==============================
                // 🔴 VALIDAR HABITACIÓN OCUPADA
                // 🔴 ==============================
                var habitacion = await _dbContext.Habitacions
                    .FirstAsync(h => h.IdHabitacion == entidad.IdHabitacion);

                if (habitacion.IdEstadoHabitacion == 3) // <-- OCUPADA
                    throw new Exception("La habitación ya está ocupada.");

                // NOTA: El check defensivo de "Recepcion activa duplicada" se removió porque:
                //  - La validación IdEstadoHabitacion == 3 (arriba) ya bloquea habitaciones realmente ocupadas.
                //  - La transacción Serializable previene race conditions entre dos check-ins simultáneos.
                //  - La validación de IdReserva único (abajo) previene re-uso de la misma reserva.
                // Este triple bloqueo es suficiente y no genera falsos positivos por datos legacy.

                // 🔴 ==============================
                // 🔴 VALIDAR CHECK-IN DUPLICADO (por reserva)
                // 🔴 ==============================
                if (entidad.IdReserva != null && entidad.IdReserva > 0)
                {
                    var yaExiste = await _dbContext.Recepcions
                        .AnyAsync(r => r.IdReserva == entidad.IdReserva);

                    if (yaExiste)
                        throw new Exception("Esta reserva ya fue utilizada para un check-in.");
                }

                if (entidad.IdClienteNavigation != null)
                {
                    // crear cliente si viene nuevo
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
                        // viene cliente existente dentro de la navigation
                        entidad.IdCliente = entidad.IdClienteNavigation.IdCliente;
                        entidad.IdClienteNavigation = null;
                    }
                }
                else
                {
                    // ✅ si no viene navigation, debe venir IdCliente
                    if (!entidad.IdCliente.HasValue || entidad.IdCliente <= 0)
                        throw new Exception("Debe enviar el cliente.");

                    // ok: ya tenemos entidad.IdCliente
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
                        (r.EstadoReserva == "RESERVADA" || r.EstadoReserva == "CONFIRMADA") &&
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
                    reserva.EstadoReserva = "CHECKIN";
                    _dbContext.Reservas.Update(reserva);
                    // NO guardo aún, lo hacemos al final
                }

                // -----------------------------
                // Habitación -> Ocupado
                // -----------------------------
                //var habitacion = await _dbContext.Habitacions
                //    .FirstAsync(h => h.IdHabitacion == entidad.IdHabitacion);

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
        
        public async Task<bool> Finalizar(int idRecepcion, DateTime fechaSalidaConfirmacion, decimal costoPenalidad)
        {
            using var tx = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var recepcion = await _dbContext.Recepcions
                    .FirstOrDefaultAsync(r => r.IdRecepcion == idRecepcion);

                if (recepcion == null)
                    throw new Exception("No se encontró la recepción.");

                if (recepcion.Estado == false)
                    throw new Exception("Esta recepción ya fue finalizada.");

                // Normalizar penalidad
                if (costoPenalidad < 0m) costoPenalidad = 0m;

                recepcion.CostoPenalidad = costoPenalidad;
                recepcion.FechaSalidaConfirmacion = fechaSalidaConfirmacion;

                // Total pagado final = PrecioInicial + Penalidad
                var baseHospedaje = recepcion.PrecioInicial ?? 0m;
                recepcion.TotalPagado = baseHospedaje + costoPenalidad;

                // Finalizar estado
                recepcion.Estado = false;

                _dbContext.Recepcions.Update(recepcion);
                await _dbContext.SaveChangesAsync();

                // Cambiar habitación a LIMPIEZA (2)
                var habitacion = await _dbContext.Habitacions
                    .FirstOrDefaultAsync(h => h.IdHabitacion == recepcion.IdHabitacion);

                if (habitacion == null)
                    throw new Exception("No se encontró la habitación asociada a la recepción.");

                habitacion.IdEstadoHabitacion = 2;
                _dbContext.Habitacions.Update(habitacion);
                await _dbContext.SaveChangesAsync();

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
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

        //public  async Task<List<Recepcion>> Reporte(string FechaInicio, string FechaFin)
        //{
        //    DateTime fech_Inicio = DateTime.ParseExact(FechaInicio, "dd/MM/yyyy", new CultureInfo("es-PE"));
        //    DateTime fech_Fin = DateTime.ParseExact(FechaFin, "dd/MM/yyyy", new CultureInfo("es-PE"));

        //    List<Recepcion> listaResumen = await _dbContext.Recepcions
        //        .Include(p => p.IdClienteNavigation)
        //        .Include(v => v.IdHabitacionNavigation)
        //         .Where(dv => dv.FechaEntrada.Value.Date >= fech_Inicio.Date && dv.FechaEntrada.Value.Date <= fech_Fin.Date)
        //        .ToListAsync();

        //    return listaResumen;
        //}
        public async Task<List<ReporteDTO>> Reporte(string FechaInicio, string FechaFin)
        {
            var fech_Inicio = DateTime.ParseExact(FechaInicio, "dd/MM/yyyy", new CultureInfo("es-PE"));
            var fech_Fin = DateTime.ParseExact(FechaFin, "dd/MM/yyyy", new CultureInfo("es-PE"));

            var lista = await _dbContext.Recepcions
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdHabitacionNavigation)
                .Where(r =>
                    r.FechaEntrada.HasValue &&
                    r.FechaEntrada.Value.Date >= fech_Inicio.Date &&
                    r.FechaEntrada.Value.Date <= fech_Fin.Date
                )
                .Select(r => new ReporteDTO
                {
                    NombreCliente = r.IdClienteNavigation.NombreCompleto,
                    TipoDocumento = r.IdClienteNavigation.TipoDocumento,
                    NroDocumento = r.IdClienteNavigation.Documento,
                    NroHabitacion = r.IdHabitacionNavigation.Numero,

                    FechaEntrada = r.FechaEntrada.Value.ToString("dd/MM/yyyy"),
                    FechaSalida = r.FechaSalida.Value.ToString("dd/MM/yyyy"),

                    Adelanto = r.Adelanto ?? 0,
                    PrecioRestante = r.PrecioRestante ?? 0,
                    MetodoPago = r.MetodoPago,
                    NotaMetodoPago = r.NotaMetodoPago,  // ✅ Detalle del método (especialmente para MIXTO)

                    CostoPenalidad = r.CostoPenalidad ?? 0,
                    TotalPagado = r.TotalPagado ?? 0,
                    Observacion = r.Observacion
                })
                .ToListAsync();

            return lista;
        }
        public async Task<bool> CambiarHabitacion(int idRecepcion, int idNuevaHabitacion)
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Obtener la recepción (sin tracking de navegaciones)
                var recepcion = await _dbContext.Recepcions
                    .FirstOrDefaultAsync(r => r.IdRecepcion == idRecepcion);

                if (recepcion == null)
                    throw new Exception("No se encontró la recepción.");

                if (recepcion.Estado == false)
                    throw new Exception("La recepción ya está finalizada.");

                if (!recepcion.IdHabitacion.HasValue || recepcion.IdHabitacion <= 0)
                    throw new Exception("La recepción no tiene una habitación asignada.");

                var idHabitacionAnterior = recepcion.IdHabitacion.Value;

                if (idHabitacionAnterior == idNuevaHabitacion)
                    throw new Exception("La nueva habitación no puede ser la misma habitación actual.");

                // 2️⃣ Validar habitaciones
                var habitacionAnterior = await _dbContext.Habitacions
                    .FirstOrDefaultAsync(h => h.IdHabitacion == idHabitacionAnterior);

                if (habitacionAnterior == null)
                    throw new Exception("No se encontró la habitación actual.");

                var habitacionNueva = await _dbContext.Habitacions
                    .FirstOrDefaultAsync(h => h.IdHabitacion == idNuevaHabitacion);

                if (habitacionNueva == null)
                    throw new Exception("No se encontró la nueva habitación.");

                // 1 = Disponible, 2 = Limpieza, 3 = Ocupada
                if (habitacionNueva.IdEstadoHabitacion != 1)
                    throw new Exception("La nueva habitación no está disponible.");

                // 3️⃣ Actualizar la RECEPCIÓN con la nueva habitación
                // EF Core ya está trackeando la entidad, basta con modificar la propiedad
                recepcion.IdHabitacion = idNuevaHabitacion;

                // 4️⃣ ✅ Actualizar la RESERVA asociada (si existe)
                // Esto evita inconsistencia: la reserva debe apuntar a la habitación actual
                if (recepcion.IdReserva.HasValue && recepcion.IdReserva.Value > 0)
                {
                    var reserva = await _dbContext.Reservas
                        .FirstOrDefaultAsync(r => r.IdReserva == recepcion.IdReserva.Value);

                    if (reserva != null)
                    {
                        reserva.IdHabitacion = idNuevaHabitacion;
                    }
                }

                // 5️⃣ Habitación anterior → Limpieza
                habitacionAnterior.IdEstadoHabitacion = 2;

                // 6️⃣ Habitación nueva → Ocupada
                habitacionNueva.IdEstadoHabitacion = 3;

                // 7️⃣ Guardar TODOS los cambios en una sola transacción
                var cambios = await _dbContext.SaveChangesAsync();

                if (cambios <= 0)
                    throw new Exception("No se aplicaron cambios en la base de datos.");

                await tx.CommitAsync();

                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
