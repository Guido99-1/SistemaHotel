using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Server.Utilidades;
using SistemaHotel.Shared;

namespace SistemaHotel.Server.Repositorio.Implementacion
{
    public class ReservaGrupoRepositorio : IReservaGrupoRepositorio
    {
        private readonly DbhotelBlazorContext _dbContext;
        private readonly IFechaService _fechaService;

        public ReservaGrupoRepositorio(DbhotelBlazorContext dbContext, IFechaService fechaService)
        {
            _dbContext = dbContext;
            _fechaService = fechaService;
        }

        public async Task<ReservaMultipleResponseDTO> CrearReservaMultiple(CrearReservaMultipleDTO request)
        {
            // ════════════════════════════════════════════════
            // 1. VALIDACIONES GENERALES
            // ════════════════════════════════════════════════
            if (request.Habitaciones == null || request.Habitaciones.Count == 0)
                throw new Exception("Debe seleccionar al menos 1 habitación");

            if (request.FechaEntrada >= request.FechaSalida)
                throw new Exception("La fecha de salida debe ser posterior a la entrada");

            int totalPersonas = request.Habitaciones.Sum(h => h.CantidadPersonas);
            if (totalPersonas == 0)
                throw new Exception("Debe ingresar cantidad de personas");

            if (request.Habitaciones.Any(h => h.CantidadPersonas <= 0))
                throw new Exception("Cada habitación debe tener al menos 1 persona");

            // Normalizar montos a 2 decimales
            decimal montoTotalNormalizado = Math.Round(request.MontoTotal, 2);
            decimal adelantoNormalizado = Math.Round(request.Adelanto, 2);

            if (montoTotalNormalizado <= 0)
                throw new Exception("El monto total debe ser mayor a 0");

            if (adelantoNormalizado < 0)
                throw new Exception("El adelanto no puede ser negativo");

            if (adelantoNormalizado > montoTotalNormalizado)
                throw new Exception("El adelanto no puede ser mayor al monto total");

            // Precio por persona (informativo, no usado para cálculo final)
            decimal precioPersona = Math.Round(montoTotalNormalizado / totalPersonas, 2);

            // ════════════════════════════════════════════════
            // 2. VALIDAR DISPONIBILIDAD DE TODAS LAS HABITACIONES
            //    (antes de crear nada)
            // ════════════════════════════════════════════════
            var habitacionesValidadas = new List<(HabitacionReservaDTO Request, Habitacion Habitacion)>();

            foreach (var habRequest in request.Habitaciones)
            {
                var habitacion = await _dbContext.Habitacions
                    .FirstOrDefaultAsync(h => h.IdHabitacion == habRequest.IdHabitacion);

                if (habitacion == null)
                    throw new Exception($"Habitación {habRequest.IdHabitacion} no existe");

                var conflicto = await ObtenerConflictoHabitacion(
                    habRequest.IdHabitacion,
                    request.FechaEntrada,
                    request.FechaSalida);

                if (conflicto != null)
                    throw new Exception($"Habitación {habitacion.Numero} no disponible: {conflicto}");

                habitacionesValidadas.Add((habRequest, habitacion));
            }

            // ════════════════════════════════════════════════
            // 3. CALCULAR DISTRIBUCIÓN EXACTA DE PRECIOS Y ADELANTOS
            //    Usa "ajuste en la última habitación" para garantizar
            //    que la suma sea EXACTAMENTE igual al monto total.
            // ════════════════════════════════════════════════

            int totalHabitaciones = habitacionesValidadas.Count;

            // Arrays para almacenar los valores calculados
            var precios = new decimal[totalHabitaciones];
            var adelantos = new decimal[totalHabitaciones];
            var pendientes = new decimal[totalHabitaciones];

            decimal sumaPreciosParcial = 0m;
            decimal sumaAdelantosParcial = 0m;

            for (int i = 0; i < totalHabitaciones; i++)
            {
                var (habRequest, habitacion) = habitacionesValidadas[i];
                bool esUltima = (i == totalHabitaciones - 1);

                decimal precioHabitacion;
                decimal adelantoHabitacion;

                if (esUltima)
                {
                    // ✅ ÚLTIMA HABITACIÓN: absorbe el redondeo para garantizar exactitud
                    precioHabitacion = montoTotalNormalizado - sumaPreciosParcial;
                    adelantoHabitacion = adelantoNormalizado - sumaAdelantosParcial;
                }
                else
                {
                    // Habitaciones intermedias: cálculo proporcional con redondeo
                    precioHabitacion = Math.Round(habRequest.CantidadPersonas * precioPersona, 2);

                    // Adelanto proporcional al precio
                    adelantoHabitacion = montoTotalNormalizado > 0
                        ? Math.Round((precioHabitacion / montoTotalNormalizado) * adelantoNormalizado, 2)
                        : 0m;

                    sumaPreciosParcial += precioHabitacion;
                    sumaAdelantosParcial += adelantoHabitacion;
                }

                // Validaciones individuales por habitación
                if (precioHabitacion < 0) precioHabitacion = 0;
                if (adelantoHabitacion < 0) adelantoHabitacion = 0;

                // ✅ El adelanto NUNCA puede ser mayor al precio de la habitación
                if (adelantoHabitacion > precioHabitacion)
                    adelantoHabitacion = precioHabitacion;

                // Pendiente = Precio - Adelanto (siempre positivo)
                decimal pendienteHabitacion = precioHabitacion - adelantoHabitacion;
                if (pendienteHabitacion < 0) pendienteHabitacion = 0;

                precios[i] = precioHabitacion;
                adelantos[i] = adelantoHabitacion;
                pendientes[i] = pendienteHabitacion;
            }

            // ════════════════════════════════════════════════
            // 4. VALIDACIÓN MATEMÁTICA: las sumas DEBEN ser exactas
            // ════════════════════════════════════════════════
            decimal sumaPreciosFinal = precios.Sum();
            decimal sumaAdelantosFinal = adelantos.Sum();
            decimal sumaPendientesFinal = pendientes.Sum();
            decimal pendienteTotal = montoTotalNormalizado - adelantoNormalizado;

            // Tolerancia mínima para errores de redondeo de decimal (centavos)
            const decimal tolerancia = 0.01m;

            if (Math.Abs(sumaPreciosFinal - montoTotalNormalizado) > tolerancia)
                throw new Exception(
                    $"Error de cálculo: suma de precios ({sumaPreciosFinal:N2}) " +
                    $"diferente al monto total ({montoTotalNormalizado:N2})");

            if (Math.Abs(sumaAdelantosFinal - adelantoNormalizado) > tolerancia)
                throw new Exception(
                    $"Error de cálculo: suma de adelantos ({sumaAdelantosFinal:N2}) " +
                    $"diferente al adelanto total ({adelantoNormalizado:N2})");

            if (Math.Abs(sumaPendientesFinal - pendienteTotal) > tolerancia)
                throw new Exception(
                    $"Error de cálculo: suma de pendientes ({sumaPendientesFinal:N2}) " +
                    $"diferente al pendiente total ({pendienteTotal:N2})");

            // ════════════════════════════════════════════════
            // 5. CREAR EL GRUPO DE RESERVA Y LAS RESERVAS
            // ════════════════════════════════════════════════
            var grupoReserva = new ReservaGrupo
            {
                IdCliente = request.IdCliente,
                FechaEntrada = request.FechaEntrada,
                FechaSalida = request.FechaSalida,
                MontoTotal = montoTotalNormalizado,
                PrecioPersona = precioPersona,
                TotalPersonas = totalPersonas,
                Observacion = request.Observacion,
                Estado = true,
                FechaCreacion = _fechaService.Now
            };

            var respuesta = new ReservaMultipleResponseDTO
            {
                IdCliente = request.IdCliente,
                FechaEntrada = request.FechaEntrada,
                FechaSalida = request.FechaSalida,
                MontoTotal = montoTotalNormalizado,
                PrecioPersona = precioPersona,
                TotalPersonas = totalPersonas,
                Observacion = request.Observacion,
                Reservas = new()
            };

            for (int i = 0; i < totalHabitaciones; i++)
            {
                var (habRequest, habitacion) = habitacionesValidadas[i];

                var reserva = new Reserva
                {
                    IdCliente = request.IdCliente,
                    IdHabitacion = habRequest.IdHabitacion,
                    FechaEntrada = request.FechaEntrada,
                    FechaSalidaReserva = request.FechaSalida,
                    PrecioInicial = precios[i],            // ✅ Suma exacta = MontoTotal
                    Adelanto = adelantos[i],               // ✅ Suma exacta = Adelanto
                    PrecioRestante = pendientes[i],        // ✅ Suma exacta = Pendiente
                    TotalPagado = adelantos[i],            // Lo cancelado hasta ahora
                    CostoPenalidad = 0,
                    Observacion = request.Observacion,
                    Estado = true,
                    EstadoReserva = "RESERVADA",
                    CantidadPersonas = habRequest.CantidadPersonas,
                    FechaCreacion = _fechaService.Now
                };

                grupoReserva.Reservas.Add(reserva);

                respuesta.Reservas.Add(new ReservaDetalleDTO
                {
                    IdHabitacion = habRequest.IdHabitacion,
                    NumeroHabitacion = habitacion.Numero,
                    CantidadPersonas = habRequest.CantidadPersonas,
                    PrecioCalculado = precios[i]
                });
            }

            // ════════════════════════════════════════════════
            // 6. GUARDAR EN BD
            // ════════════════════════════════════════════════
            _dbContext.ReservaGrupos.Add(grupoReserva);
            await _dbContext.SaveChangesAsync();

            respuesta.IdReservaGrupo = grupoReserva.IdReservaGrupo;

            return respuesta;
        }

        public async Task<ReservaGrupo?> ObtenerGrupo(int idReservaGrupo)
        {
            return await _dbContext.ReservaGrupos
                .Include(g => g.Reservas)
                .Include(g => g.IdClienteNavigation)
                .FirstOrDefaultAsync(g => g.IdReservaGrupo == idReservaGrupo);
        }

        public async Task<List<ReservaGrupo>> ObtenerPorCliente(int idCliente)
        {
            return await _dbContext.ReservaGrupos
                .Where(g => g.IdCliente == idCliente && g.Estado == true)
                .Include(g => g.Reservas)
                .OrderByDescending(g => g.FechaCreacion)
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve null si la habitación está disponible.
        /// Devuelve un mensaje detallado si hay conflicto (qué tipo, IDs, fechas).
        /// </summary>
        private async Task<string?> ObtenerConflictoHabitacion(
            int idHabitacion,
            DateTime entrada,
            DateTime salida)
        {
            // Normalizar fechas (solo fecha, sin hora)
            var fechaEntrada = entrada.Date;
            var fechaSalida = salida.Date;

            // ✅ 1. Validar cruces con RESERVAS activas
            var estadosBloqueantes = new[] { "RESERVADA", "CONFIRMADA", "CHECKIN" };

            var reservaCruce = await _dbContext.Reservas
                .Where(r =>
                    r.IdHabitacion == idHabitacion &&
                    r.Estado == true &&
                    estadosBloqueantes.Contains(r.EstadoReserva) &&
                    r.FechaEntrada.HasValue &&
                    r.FechaSalidaReserva.HasValue &&
                    fechaEntrada < r.FechaSalidaReserva.Value.Date &&
                    fechaSalida > r.FechaEntrada.Value.Date
                )
                .Select(r => new
                {
                    r.IdReserva,
                    r.EstadoReserva,
                    FechaInicio = r.FechaEntrada,
                    FechaFin = r.FechaSalidaReserva
                })
                .FirstOrDefaultAsync();

            if (reservaCruce != null)
            {
                return $"Existe RESERVA #{reservaCruce.IdReserva} ({reservaCruce.EstadoReserva}) " +
                       $"del {reservaCruce.FechaInicio:dd/MM/yyyy} al {reservaCruce.FechaFin:dd/MM/yyyy}";
            }

            // ✅ 2. Validar cruces con RECEPCIONES activas (check-ins en curso)
            var recepcionCruce = await _dbContext.Recepcions
                .Where(r =>
                    r.IdHabitacion == idHabitacion &&
                    r.Estado == true &&
                    r.FechaEntrada.HasValue &&
                    r.FechaSalida.HasValue &&
                    fechaEntrada < r.FechaSalida.Value.Date &&
                    fechaSalida > r.FechaEntrada.Value.Date
                )
                .Select(r => new
                {
                    r.IdRecepcion,
                    FechaInicio = r.FechaEntrada,
                    FechaFin = r.FechaSalida
                })
                .FirstOrDefaultAsync();

            if (recepcionCruce != null)
            {
                return $"Existe CHECK-IN ACTIVO #{recepcionCruce.IdRecepcion} " +
                       $"del {recepcionCruce.FechaInicio:dd/MM/yyyy} al {recepcionCruce.FechaFin:dd/MM/yyyy}";
            }

            // No hay conflicto
            return null;
        }
    }
}
