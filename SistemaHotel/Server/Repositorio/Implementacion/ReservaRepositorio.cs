using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;


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

        //public async Task<bool> Editar(Reserva entidad)
        //{
        //    await using var transaction = await _dbContext.Database
        //        .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        //    try
        //    {
        //        if (entidad.IdReserva <= 0)
        //            throw new InvalidOperationException("IdReserva inválido.");

        //        if (entidad.IdHabitacion == null || entidad.IdHabitacion <= 0)
        //            throw new InvalidOperationException("Debe seleccionar una habitación.");

        //        if (entidad.IdCliente == null || entidad.IdCliente <= 0)
        //            throw new InvalidOperationException("Debe seleccionar un cliente.");

        //        if (!entidad.FechaEntrada.HasValue || !entidad.FechaSalidaReserva.HasValue)
        //            throw new InvalidOperationException("Debe enviar FechaEntrada y FechaSalidaReserva.");

        //        var entrada = entidad.FechaEntrada.Value.Date;
        //        var salida = entidad.FechaSalidaReserva.Value.Date;

        //        if (salida <= entrada)
        //            throw new InvalidOperationException("La fecha de salida debe ser mayor a la fecha de entrada.");

        //        // ✅ Solapamiento excluyendo esta misma reserva
        //        var haySolapamiento = await ExisteSolapamiento(
        //            entidad.IdHabitacion.Value,
        //            entrada,
        //            salida,
        //            excluirIdReserva: entidad.IdReserva
        //        );

        //        if (haySolapamiento)
        //            throw new InvalidOperationException(
        //                $"Habitación no disponible. Ya está ocupada o reservada entre {entrada:dd/MM/yyyy} y {salida:dd/MM/yyyy}. " +
        //                "Elige otras fechas o selecciona otra habitación."
        //            );

        //        // No navegar
        //        entidad.IdHabitacionNavigation = null;
        //        entidad.IdClienteNavigation = null;

        //        entidad.Adelanto ??= 0;
        //        entidad.Estado = true;
        //        entidad.EstadoReserva = string.IsNullOrWhiteSpace(entidad.EstadoReserva) ? "RESERVADA" : entidad.EstadoReserva;

        //        _dbContext.Reservas.Update(entidad);
        //        await _dbContext.SaveChangesAsync();

        //        await transaction.CommitAsync();
        //        return true;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}



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

        public async Task<bool> EditarReserva(Reserva entidad)
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                if (entidad.IdReserva <= 0)
                    throw new InvalidOperationException("IdReserva inválido.");

                var reserva = await _dbContext.Reservas
                    .FirstOrDefaultAsync(r => r.IdReserva == entidad.IdReserva);

                if (reserva == null)
                    throw new Exception("No se encontró la reserva.");
                // ✅ Cambiar habitación (permitido solo si viene)
                if (entidad.IdHabitacion is not null && entidad.IdHabitacion > 0)
                {
                    if (!entidad.FechaEntrada.HasValue || !entidad.FechaSalidaReserva.HasValue)
                        throw new InvalidOperationException("Debe enviar FechaEntrada y FechaSalidaReserva.");

                    var entrada = entidad.FechaEntrada.Value.Date;
                    var salida = entidad.FechaSalidaReserva.Value.Date;

                    if (salida <= entrada)
                        throw new InvalidOperationException("La fecha de salida debe ser mayor a la fecha de entrada.");

                    // ✅ VALIDAR CON LA HABITACIÓN NUEVA
                    var haySolapamiento = await ExisteSolapamiento(
                        entidad.IdHabitacion.Value,      // ✅ AQUÍ
                        entrada,
                        salida,
                        excluirIdReserva: reserva.IdReserva
                    );

                    // ✅ VALIDAR CON LA HABITACIÓN NUEVA

                    if (haySolapamiento)
                    {
                        // Traer número de la habitación (C02)
                        var hab = await _dbContext.Habitacions
                            .AsNoTracking()
                            .FirstOrDefaultAsync(h => h.IdHabitacion == entidad.IdHabitacion.Value);

                        var nroHab = hab?.Numero ?? entidad.IdHabitacion.Value.ToString();

                        // 1) Buscar si el cruce viene de una RECEPCIÓN ACTIVA
                        var recepcionCruce = await _dbContext.Recepcions
                            .AsNoTracking()
                            .Include(r => r.IdClienteNavigation)
                            .Where(r =>
                                r.IdHabitacion == entidad.IdHabitacion.Value &&
                                r.Estado == true &&
                                r.FechaEntrada.HasValue &&
                                r.FechaSalida.HasValue &&
                                entrada < r.FechaSalida.Value.Date &&
                                salida > r.FechaEntrada.Value.Date
                            )
                            .OrderByDescending(r => r.IdRecepcion)
                            .FirstOrDefaultAsync();

                        if (recepcionCruce != null)
                        {
                            var cli = recepcionCruce.IdClienteNavigation?.NombreCompleto ?? "otro huésped";
                            var fi = recepcionCruce.FechaEntrada!.Value.ToString("dd/MM/yyyy");
                            var ff = recepcionCruce.FechaSalida!.Value.ToString("dd/MM/yyyy");

                            throw new InvalidOperationException(
                                $"No se puede cambiar a la habitación {nroHab} porque está OCUPADA (recepción activa) " +
                                $"por {cli} en el rango {fi} - {ff}. " +
                                $"Selecciona otra habitación o cambia las fechas."
                            );
                        }

                        // 2) Si no es recepción, entonces es una RESERVA ACTIVA
                        var reservaCruce = await _dbContext.Reservas
                            .AsNoTracking()
                            .Include(r => r.IdClienteNavigation)
                            .Where(r =>
                                r.IdHabitacion == entidad.IdHabitacion.Value &&
                                r.Estado == true &&
                                r.IdReserva != reserva.IdReserva &&
                                r.FechaEntrada.HasValue &&
                                r.FechaSalidaReserva.HasValue &&
                                entrada < r.FechaSalidaReserva.Value.Date &&
                                salida > r.FechaEntrada.Value.Date
                            )
                            .OrderByDescending(r => r.IdReserva)
                            .FirstOrDefaultAsync();

                        if (reservaCruce != null)
                        {
                            var cli = reservaCruce.IdClienteNavigation?.NombreCompleto ?? "otro cliente";
                            var fi = reservaCruce.FechaEntrada!.Value.ToString("dd/MM/yyyy");
                            var ff = reservaCruce.FechaSalidaReserva!.Value.ToString("dd/MM/yyyy");

                            throw new InvalidOperationException(
                                $"No se puede cambiar a la habitación {nroHab} porque ya existe una RESERVA " +
                                $"para {cli} en el rango {fi} - {ff}. " +
                                $"Selecciona otra habitación o cambia las fechas."
                            );
                        }

                        // fallback (por si no encuentra el causante)
                        throw new InvalidOperationException(
                            $"No se puede cambiar a la habitación {nroHab} porque no está disponible " +
                            $"para el rango {entrada:dd/MM/yyyy} - {salida:dd/MM/yyyy}."
                        );
                    }

                    // ✅ ACTUALIZAR HABITACIÓN
                    reserva.IdHabitacion = entidad.IdHabitacion.Value;  // ✅ AQUÍ

                    // ✅ Actualizar campos permitidos
                    reserva.FechaEntrada = entrada;
                    reserva.FechaSalidaReserva = salida;

                    var precio = entidad.PrecioInicial ?? 0m;
                    var adelanto = entidad.Adelanto ?? 0m;

                    if (precio < 0m) precio = 0m;
                    if (adelanto < 0m) adelanto = 0m;
                    if (adelanto > precio) adelanto = precio;

                    reserva.PrecioInicial = precio;
                    reserva.Adelanto = adelanto;

                    reserva.PrecioRestante = Math.Max(0m, precio - adelanto);

                    reserva.Observacion = entidad.Observacion;
                }
                // EstadoReserva (si lo manejas)
                if (!string.IsNullOrWhiteSpace(entidad.EstadoReserva))
                    reserva.EstadoReserva = entidad.EstadoReserva;

                // ⚠️ NO recomiendo cambiar Estado aquí a true siempre.
                // Si quieres permitirlo:
                // reserva.Estado = entidad.Estado;

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
        public async Task<bool> Eliminar(int idReserva)
        {
            try
            {
                var reserva = await _dbContext.Reservas
                    .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

                if (reserva == null)
                    throw new Exception("No se encontró la reserva.");

                _dbContext.Reservas.Remove(reserva);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Esto normalmente es FK constraint
                var msg = ex.InnerException?.Message ?? ex.Message;
                throw new Exception("No se puede eliminar porque la reserva tiene registros relacionados. Detalle: " + msg);
            }
        }
    }
}
