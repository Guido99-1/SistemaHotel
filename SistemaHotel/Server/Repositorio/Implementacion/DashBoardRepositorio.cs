using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Server.Utilidades;
using SistemaHotel.Shared;
using System.Globalization;
namespace SistemaHotel.Server.Repositorio.Implementacion
{
    public class DashBoardRepositorio : IDashBoardRepositorio
    {
        private readonly DbhotelBlazorContext _dbContext;
        private readonly IFechaService _fechaService;

        public DashBoardRepositorio(DbhotelBlazorContext dbContext, IFechaService fechaService)
        {
            _dbContext = dbContext;
            _fechaService = fechaService;
        }
        public async Task<List<OcupacionDiaDTO>> OcupacionMes()
        {
            var hoy = _fechaService.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var ocupacion = await _dbContext.Recepcions
                .Where(r => r.FechaEntrada.HasValue
                            && r.FechaEntrada.Value.Date >= inicioMes
                            && r.FechaEntrada.Value.Date <= hoy)
                .GroupBy(r => r.FechaEntrada!.Value.Date)
                .Select(g => new OcupacionDiaDTO
                {
                    Fecha = g.Key.ToString("dd/MM/yyyy"),
                    FechaDate = g.Key,
                    Ocupadas = g.Count()
                })
                .ToListAsync();

            // ✅ Rellenar días faltantes con 0 (para gráfico bonito)
            var dias = Enumerable.Range(0, (hoy - inicioMes).Days + 1)
                .Select(i => inicioMes.AddDays(i))
                .ToList();

            var dic = ocupacion.ToDictionary(x => x.Fecha, x => x.Ocupadas);

            return dias.Select(d => new OcupacionDiaDTO
            {
                Fecha = d.ToString("dd/MM/yyyy"),
                FechaDate = d,                // ✅ AQUÍ
                Ocupadas = dic.TryGetValue(d.ToString("dd/MM/yyyy"), out var v) ? v : 0
            }).ToList();
        }

        public async Task<List<IngresoDiaDTO>> IngresosMesCheckout()
        {
            var hoy = _fechaService.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var ingresos = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue
                            && r.TotalPagado.HasValue
                            && r.FechaSalidaConfirmacion.Value.Date >= inicioMes
                            && r.FechaSalidaConfirmacion.Value.Date <= hoy)
                .GroupBy(r => r.FechaSalidaConfirmacion!.Value.Date)
                .Select(g => new IngresoDiaDTO
                {
                    Fecha = g.Key.ToString("dd/MM/yyyy"),
                    FechaDate = g.Key,                 // ✅ IMPORTANTÍSIMO
                    Monto = g.Sum(x => x.TotalPagado!.Value)
                })
                .ToListAsync();

            // ✅ Rellenar días faltantes con 0
            var dias = Enumerable.Range(0, (hoy - inicioMes).Days + 1)
                .Select(i => inicioMes.AddDays(i))
                .ToList();

            var dic = ingresos.ToDictionary(x => x.FechaDate.Date, x => x.Monto);

            return dias.Select(d => new IngresoDiaDTO
            {
                Fecha = d.ToString("dd/MM/yyyy"),
                FechaDate = d,                       // ✅ IMPORTANTÍSIMO
                Monto = dic.TryGetValue(d.Date, out var v) ? v : 0m
            }).ToList();
        }


        public async Task<int> HabitacionesDisponibles()
        {
            try
            {
                IQueryable<Habitacion> query = _dbContext.Habitacions;
                int total = query.Where(h => h.IdEstadoHabitacion == 1).Count();
                return total;
            }
            catch
            {
                throw;
            }
        }
        public async Task<DashBoardDTO> ResumenDashboard()
        {
            var hoy = _fechaService.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesAnterior = inicioMes.AddMonths(-1);
            var finMesAnterior = inicioMes.AddDays(-1);
            var inicioAnio = new DateTime(hoy.Year, 1, 1);

            // ════════════════════════════════════════════════════
            // 1) KPIs PRINCIPALES
            // ════════════════════════════════════════════════════
            var dto = new DashBoardDTO
            {
                TotalHabitaciones = await TotalHabitaciones(),
                TotalHabitacionesDisponibles = await HabitacionesDisponibles(),
                TotalHabitacionesOcupadas = await HabitacionesOcupadas(),
                TotalHabitacionesEnLimpieza = await HabitacionesLimpieza(),
                TotalReservasHoy = await TotalReservasHoy(),
                TotalReservasMes = await TotalReservasMes(),
            };

            // Tasa de ocupación actual
            dto.TasaOcupacionActual = dto.TotalHabitaciones > 0
                ? (dto.TotalHabitacionesOcupadas * 100.0) / dto.TotalHabitaciones
                : 0;

            // Total clientes
            dto.TotalClientes = await _dbContext.Clientes.CountAsync();

            // Clientes nuevos este mes
            dto.ClientesNuevosMes = await _dbContext.Clientes
                .Where(c => c.FechaCreacion.HasValue &&
                            c.FechaCreacion.Value.Date >= inicioMes &&
                            c.FechaCreacion.Value.Date <= hoy)
                .CountAsync();

            // Reservas pendientes para hoy (no han hecho check-in)
            dto.ReservasPendientesHoy = await _dbContext.Reservas
                .Where(r => r.FechaEntrada.HasValue &&
                            r.FechaEntrada.Value.Date == hoy &&
                            r.Estado == true &&
                            (r.EstadoReserva == "RESERVADA" || r.EstadoReserva == "CONFIRMADA"))
                .CountAsync();

            // Check-ins de hoy
            dto.CheckInsHoy = await _dbContext.Recepcions
                .Where(r => r.FechaEntrada.HasValue &&
                            r.FechaEntrada.Value.Date == hoy)
                .CountAsync();

            // Check-outs de hoy
            dto.CheckOutsHoy = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                            r.FechaSalidaConfirmacion.Value.Date == hoy)
                .CountAsync();

            // ════════════════════════════════════════════════════
            // 2) INGRESOS Y FINANZAS
            // ════════════════════════════════════════════════════
            dto.IngresosHoy = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                            r.FechaSalidaConfirmacion.Value.Date == hoy)
                .SumAsync(r => r.TotalPagado ?? 0m);

            dto.IngresosMes = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                            r.FechaSalidaConfirmacion.Value.Date >= inicioMes &&
                            r.FechaSalidaConfirmacion.Value.Date <= hoy)
                .SumAsync(r => r.TotalPagado ?? 0m);

            dto.IngresosMesAnterior = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                            r.FechaSalidaConfirmacion.Value.Date >= inicioMesAnterior &&
                            r.FechaSalidaConfirmacion.Value.Date <= finMesAnterior)
                .SumAsync(r => r.TotalPagado ?? 0m);

            dto.IngresosAnio = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                            r.FechaSalidaConfirmacion.Value.Date >= inicioAnio &&
                            r.FechaSalidaConfirmacion.Value.Date <= hoy)
                .SumAsync(r => r.TotalPagado ?? 0m);

            // Adelantos pendientes (en reservas activas)
            dto.AdelantosPendientes = await _dbContext.Reservas
                .Where(r => r.Estado == true &&
                            (r.EstadoReserva == "RESERVADA" || r.EstadoReserva == "CONFIRMADA"))
                .SumAsync(r => r.Adelanto ?? 0m);

            // Penalidades del mes
            dto.PenalidadesMes = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                            r.FechaSalidaConfirmacion.Value.Date >= inicioMes &&
                            r.FechaSalidaConfirmacion.Value.Date <= hoy)
                .SumAsync(r => r.CostoPenalidad ?? 0m);

            // Promedio por reserva (mes)
            int countMes = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                            r.FechaSalidaConfirmacion.Value.Date >= inicioMes &&
                            r.FechaSalidaConfirmacion.Value.Date <= hoy)
                .CountAsync();

            dto.PromedioPorReserva = countMes > 0 ? dto.IngresosMes / countMes : 0;

            int diasTranscurridos = (hoy - inicioMes).Days + 1;
            dto.PromedioIngresoDia = diasTranscurridos > 0 ? dto.IngresosMes / diasTranscurridos : 0;

            // ════════════════════════════════════════════════════
            // 3) SERIES DE TIEMPO (Ocupación e Ingresos diarios)
            // ════════════════════════════════════════════════════
            var ocupacion = await _dbContext.Recepcions
                .Where(r => r.FechaEntrada.HasValue
                            && r.FechaEntrada.Value.Date >= inicioMes
                            && r.FechaEntrada.Value.Date <= hoy)
                .GroupBy(r => r.FechaEntrada!.Value.Date)
                .Select(g => new OcupacionDiaDTO
                {
                    Fecha = g.Key.ToString("dd/MM/yyyy"),
                    FechaDate = g.Key,
                    Ocupadas = g.Count()
                })
                .ToListAsync();

            var dias = Enumerable.Range(0, (hoy - inicioMes).Days + 1)
                .Select(i => inicioMes.AddDays(i))
                .ToList();

            var dic = ocupacion.ToDictionary(x => x.Fecha, x => x.Ocupadas);

            dto.OcupacionMes = dias.Select(d => new OcupacionDiaDTO
            {
                Fecha = d.ToString("dd/MM/yyyy"),
                FechaDate = d,
                Ocupadas = dic.TryGetValue(d.ToString("dd/MM/yyyy"), out var v) ? v : 0
            }).ToList();

            // Tasa promedio de ocupación del mes
            dto.NochesOcupadasMes = ocupacion.Sum(x => x.Ocupadas);
            dto.TasaOcupacionPromedioMes = (dto.TotalHabitaciones > 0 && diasTranscurridos > 0)
                ? (dto.NochesOcupadasMes * 100.0) / (dto.TotalHabitaciones * diasTranscurridos)
                : 0;

            // Ingresos por día
            var ingresosRaw = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue
                            && r.FechaSalidaConfirmacion.Value.Date >= inicioMes
                            && r.FechaSalidaConfirmacion.Value.Date <= hoy)
                .GroupBy(r => r.FechaSalidaConfirmacion!.Value.Date)
                .Select(g => new IngresoDiaDTO
                {
                    Fecha = g.Key.ToString("dd/MM/yyyy"),
                    FechaDate = g.Key,
                    Monto = g.Sum(x => x.TotalPagado ?? 0m)
                })
                .ToListAsync();

            var dicIng = ingresosRaw.ToDictionary(x => x.FechaDate.Date, x => x.Monto);

            dto.IngresosMesCheckout = dias.Select(d => new IngresoDiaDTO
            {
                Fecha = d.ToString("dd/MM/yyyy"),
                FechaDate = d,
                Monto = dicIng.TryGetValue(d.Date, out var m) ? m : 0m
            }).ToList();

            // ════════════════════════════════════════════════════
            // 4) ANÁLISIS POR CATEGORÍA
            // ════════════════════════════════════════════════════
            dto.OcupacionPorCategoria = await _dbContext.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .GroupBy(h => h.IdCategoriaNavigation!.Descripcion)
                .Select(g => new OcupacionPorCategoriaDTO
                {
                    Categoria = g.Key ?? "Sin categoría",
                    Total = g.Count(),
                    Ocupadas = g.Count(h => h.IdEstadoHabitacion == 3),
                    Disponibles = g.Count(h => h.IdEstadoHabitacion == 1)
                })
                .ToListAsync();

            // Calcular porcentaje y agregar ingresos por categoría
            foreach (var cat in dto.OcupacionPorCategoria)
            {
                cat.Porcentaje = cat.Total > 0 ? (cat.Ocupadas * 100.0) / cat.Total : 0;

                cat.IngresoMes = await _dbContext.Recepcions
                    .Include(r => r.IdHabitacionNavigation!)
                    .ThenInclude(h => h.IdCategoriaNavigation)
                    .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                                r.FechaSalidaConfirmacion.Value.Date >= inicioMes &&
                                r.FechaSalidaConfirmacion.Value.Date <= hoy &&
                                r.IdHabitacionNavigation!.IdCategoriaNavigation!.Descripcion == cat.Categoria)
                    .SumAsync(r => r.TotalPagado ?? 0m);
            }

            // ════════════════════════════════════════════════════
            // 5) ANÁLISIS POR PISO
            // ════════════════════════════════════════════════════
            dto.OcupacionPorPiso = await _dbContext.Habitacions
                .Include(h => h.IdPisoNavigation)
                .GroupBy(h => h.IdPisoNavigation!.Descripcion)
                .Select(g => new OcupacionPorPisoDTO
                {
                    Piso = g.Key ?? "Sin piso",
                    TotalHabitaciones = g.Count(),
                    HabitacionesOcupadas = g.Count(h => h.IdEstadoHabitacion == 3),
                    TasaOcupacion = g.Count() > 0 ? (g.Count(h => h.IdEstadoHabitacion == 3) * 100.0) / g.Count() : 0
                })
                .ToListAsync();

            // ════════════════════════════════════════════════════
            // 6) DISTRIBUCIÓN DE MÉTODOS DE PAGO
            // ════════════════════════════════════════════════════
            var metodosRaw = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                            r.FechaSalidaConfirmacion.Value.Date >= inicioMes &&
                            r.FechaSalidaConfirmacion.Value.Date <= hoy &&
                            !string.IsNullOrEmpty(r.MetodoPago))
                .GroupBy(r => r.MetodoPago)
                .Select(g => new MetodoPagoDTO
                {
                    Metodo = g.Key ?? "Sin definir",
                    Cantidad = g.Count(),
                    MontoTotal = g.Sum(x => x.TotalPagado ?? 0m)
                })
                .ToListAsync();

            decimal totalMetodos = metodosRaw.Sum(m => m.MontoTotal);
            foreach (var m in metodosRaw)
            {
                m.Porcentaje = totalMetodos > 0 ? (double)(m.MontoTotal * 100m / totalMetodos) : 0;
            }
            dto.DistribucionMetodosPago = metodosRaw;

            // ════════════════════════════════════════════════════
            // 7) TOP 5 CLIENTES (por gastos)
            // ════════════════════════════════════════════════════
            dto.TopClientes = await _dbContext.Recepcions
                .Include(r => r.IdClienteNavigation)
                .Where(r => r.FechaSalidaConfirmacion.HasValue &&
                            r.IdClienteNavigation != null)
                .GroupBy(r => new
                {
                    r.IdCliente,
                    Nombre = r.IdClienteNavigation!.NombreCompleto
                })
                .Select(g => new TopClienteDTO
                {
                    IdCliente = g.Key.IdCliente ?? 0,
                    Nombre = g.Key.Nombre ?? "",
                    CantidadReservas = g.Count(),
                    TotalGastado = g.Sum(x => x.TotalPagado ?? 0m)
                })
                .OrderByDescending(x => x.TotalGastado)
                .Take(5)
                .ToListAsync();

            // ════════════════════════════════════════════════════
            // 8) TOP 5 HABITACIONES (más ocupadas)
            // ════════════════════════════════════════════════════
            dto.TopHabitaciones = await _dbContext.Recepcions
                .Include(r => r.IdHabitacionNavigation!)
                .ThenInclude(h => h.IdCategoriaNavigation)
                .Where(r => r.IdHabitacionNavigation != null)
                .GroupBy(r => new
                {
                    r.IdHabitacion,
                    Numero = r.IdHabitacionNavigation!.Numero,
                    Categoria = r.IdHabitacionNavigation.IdCategoriaNavigation!.Descripcion
                })
                .Select(g => new TopHabitacionDTO
                {
                    IdHabitacion = g.Key.IdHabitacion ?? 0,
                    Numero = g.Key.Numero ?? "",
                    Categoria = g.Key.Categoria ?? "",
                    VecesOcupada = g.Count(),
                    IngresoTotal = g.Sum(x => x.TotalPagado ?? 0m)
                })
                .OrderByDescending(x => x.VecesOcupada)
                .Take(5)
                .ToListAsync();

            // ════════════════════════════════════════════════════
            // 9) PRÓXIMOS CHECK-INS (próximos 7 días)
            // ════════════════════════════════════════════════════
            var fin7Dias = hoy.AddDays(7);
            dto.ProximosCheckIns = await _dbContext.Reservas
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdHabitacionNavigation)
                .Where(r => r.Estado == true &&
                            (r.EstadoReserva == "RESERVADA" || r.EstadoReserva == "CONFIRMADA") &&
                            r.FechaEntrada.HasValue &&
                            r.FechaEntrada.Value.Date >= hoy &&
                            r.FechaEntrada.Value.Date <= fin7Dias)
                .OrderBy(r => r.FechaEntrada)
                .Take(10)
                .Select(r => new ReservaProximaDTO
                {
                    IdReserva = r.IdReserva,
                    Cliente = r.IdClienteNavigation!.NombreCompleto ?? "",
                    Habitacion = r.IdHabitacionNavigation!.Numero ?? "",
                    FechaEntrada = r.FechaEntrada!.Value,
                    FechaSalida = r.FechaSalidaReserva ?? r.FechaEntrada.Value.AddDays(1),
                    Estado = r.EstadoReserva,
                    Total = r.PrecioInicial ?? 0m
                })
                .ToListAsync();

            // ════════════════════════════════════════════════════
            // 10) DISTRIBUCIÓN DE ESTADOS DE RESERVA
            // ════════════════════════════════════════════════════
            var estadosReservas = await _dbContext.Reservas
                .Where(r => r.FechaEntrada.HasValue &&
                            r.FechaEntrada.Value.Date >= inicioMes &&
                            r.FechaEntrada.Value.Date <= hoy)
                .GroupBy(r => r.EstadoReserva)
                .Select(g => new { Estado = g.Key, Total = g.Count() })
                .ToListAsync();

            dto.ReservasReservadas = estadosReservas.FirstOrDefault(x => x.Estado == "RESERVADA")?.Total ?? 0;
            dto.ReservasConfirmadas = estadosReservas.FirstOrDefault(x => x.Estado == "CONFIRMADA")?.Total ?? 0;
            dto.ReservasFinalizadas = estadosReservas.FirstOrDefault(x => x.Estado == "FINALIZADA")?.Total ?? 0;
            dto.ReservasCanceladas = estadosReservas.FirstOrDefault(x => x.Estado == "CANCELADA")?.Total ?? 0;

            // ════════════════════════════════════════════════════
            // 11) RESERVAS DE GERENCIA (filtradas por observación)
            // ════════════════════════════════════════════════════
            // Las reservas de gerencia tienen "GERENCIA" al inicio de la observación
            var reservasGerenciaMes = await _dbContext.Reservas
                .Where(r => r.Observacion != null &&
                            r.Observacion.StartsWith("GERENCIA") &&
                            r.FechaCreacion.HasValue &&
                            r.FechaCreacion.Value.Date >= inicioMes &&
                            r.FechaCreacion.Value.Date <= hoy)
                .ToListAsync();

            dto.TotalReservasGerenciaMes = reservasGerenciaMes.Count;
            dto.IngresosGerenciaMes = reservasGerenciaMes.Sum(r => r.PrecioInicial ?? 0m);

            // Año actual
            var reservasGerenciaAnio = await _dbContext.Reservas
                .Where(r => r.Observacion != null &&
                            r.Observacion.StartsWith("GERENCIA") &&
                            r.FechaCreacion.HasValue &&
                            r.FechaCreacion.Value.Date >= inicioAnio &&
                            r.FechaCreacion.Value.Date <= hoy)
                .ToListAsync();

            dto.TotalReservasGerenciaAnio = reservasGerenciaAnio.Count;
            dto.IngresosGerenciaAnio = reservasGerenciaAnio.Sum(r => r.PrecioInicial ?? 0m);

            // Mes anterior
            dto.IngresosGerenciaMesAnterior = await _dbContext.Reservas
                .Where(r => r.Observacion != null &&
                            r.Observacion.StartsWith("GERENCIA") &&
                            r.FechaCreacion.HasValue &&
                            r.FechaCreacion.Value.Date >= inicioMesAnterior &&
                            r.FechaCreacion.Value.Date <= finMesAnterior)
                .SumAsync(r => r.PrecioInicial ?? 0m);

            // Detección de festivos vs normales (basado en tarifa $20 = festivo, $15 = normal)
            // En la observación NO está el detalle, usamos cantidad de personas + precio
            // Para simplificar: si el precio total dividido por personas y noches es 20, es festivo
            foreach (var r in reservasGerenciaMes)
            {
                int personas = r.CantidadPersonas > 0 ? r.CantidadPersonas : 1;
                int noches = (r.FechaSalidaReserva.HasValue && r.FechaEntrada.HasValue)
                    ? Math.Max(1, (r.FechaSalidaReserva.Value.Date - r.FechaEntrada.Value.Date).Days)
                    : 1;

                decimal tarifa = (r.PrecioInicial ?? 0m) / (personas * noches);

                if (Math.Abs(tarifa - 20m) < 0.5m)
                    dto.ReservasGerenciaFestivo++;
                else
                    dto.ReservasGerenciaNormal++;
            }

            // Ingresos diarios de gerencia (mes)
            var ingresosGerenciaPorDia = reservasGerenciaMes
                .Where(r => r.FechaCreacion.HasValue)
                .GroupBy(r => r.FechaCreacion!.Value.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.PrecioInicial ?? 0m));

            dto.IngresosGerenciaDiarios = dias.Select(d => new IngresoDiaDTO
            {
                Fecha = d.ToString("dd/MM/yyyy"),
                FechaDate = d,
                Monto = ingresosGerenciaPorDia.TryGetValue(d.Date, out var m) ? m : 0m
            }).ToList();

            // Reservas de gerencia recientes (últimas 10)
            dto.ReservasGerenciaRecientes = await _dbContext.Reservas
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdHabitacionNavigation)
                .Where(r => r.Observacion != null &&
                            r.Observacion.StartsWith("GERENCIA"))
                .OrderByDescending(r => r.FechaCreacion)
                .Take(10)
                .Select(r => new ReservaProximaDTO
                {
                    IdReserva = r.IdReserva,
                    Cliente = r.IdClienteNavigation!.NombreCompleto ?? "",
                    Habitacion = r.IdHabitacionNavigation!.Numero ?? "",
                    FechaEntrada = r.FechaEntrada ?? DateTime.MinValue,
                    FechaSalida = r.FechaSalidaReserva ?? DateTime.MinValue,
                    Estado = r.EstadoReserva,
                    Total = r.PrecioInicial ?? 0m
                })
                .ToListAsync();

            return dto;
        }
        public async Task<int> HabitacionesLimpieza()
        {
            try
            {
                IQueryable<Habitacion> query = _dbContext.Habitacions;
                int total = query.Where(h => h.IdEstadoHabitacion == 2).Count();
                return total;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> HabitacionesOcupadas()
        {
            try
            {
                IQueryable<Habitacion> query = _dbContext.Habitacions;
                int total = query.Where(h => h.IdEstadoHabitacion == 3).Count();
                return total;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> TotalHabitaciones()
        {
            return await _dbContext.Habitacions.CountAsync(h => h.Estado == true);
        }
        public async Task<int> TotalReservasHoy()
        {
            var hoy = _fechaService.Today;

            return await _dbContext.Reservas
                .Where(r => r.FechaEntrada.HasValue &&
                            r.FechaEntrada.Value.Date == hoy)
                .CountAsync();
        }

        public async Task<int> TotalReservasMes()
        {
            var hoy = _fechaService.Today;

            return await _dbContext.Reservas
                .Where(r => r.FechaEntrada.HasValue &&
                            r.FechaEntrada.Value.Month == hoy.Month &&
                            r.FechaEntrada.Value.Year == hoy.Year)
                .CountAsync();
        }

    }
}