using Microsoft.EntityFrameworkCore;
using SistemaHotel.Server.Models;
using SistemaHotel.Server.Repositorio.Contratos;
using SistemaHotel.Shared;
using System.Globalization;
namespace SistemaHotel.Server.Repositorio.Implementacion
{
    public class DashBoardRepositorio : IDashBoardRepositorio
    {
        private readonly DbhotelBlazorContext _dbContext;

        public DashBoardRepositorio(DbhotelBlazorContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<OcupacionDiaDTO>> OcupacionMes()
        {
            var hoy = DateTime.Today;
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
            var hoy = DateTime.Today;
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
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            //var dto = new DashBoardDTO();

            // ... aquí tu lógica actual (totales habitaciones, etc)
            var dto = new DashBoardDTO
            {
                TotalHabitaciones = await TotalHabitaciones(),
                TotalHabitacionesDisponibles = await HabitacionesDisponibles(),
                TotalHabitacionesOcupadas = await HabitacionesOcupadas(),
                TotalHabitacionesEnLimpieza = await HabitacionesLimpieza(),
                TotalReservasHoy = await TotalReservasHoy(),
                TotalReservasMes = await TotalReservasMes(),
            };
            // ✅ Ocupación por día del mes (check-ins)
            var ocupacion = await _dbContext.Recepcions
                .Where(r => r.FechaEntrada.HasValue
                            && r.FechaEntrada.Value.Date >= inicioMes
                            && r.FechaEntrada.Value.Date <= hoy)
                .GroupBy(r => r.FechaEntrada!.Value.Date)
                .Select(g => new OcupacionDiaDTO
                {
                    Fecha = g.Key.ToString("dd/MM/yyyy"),
                    FechaDate = g.Key,           // ✅ AQUÍ
                    Ocupadas = g.Count()
                })
                //.OrderBy(x => DateTime.ParseExact(x.Fecha, "dd/MM/yyyy", new CultureInfo("es-PE")))
                .ToListAsync();

            // ✅ Si quieres que aparezcan días con 0 (recomendado para el gráfico)
            var dias = Enumerable.Range(0, (hoy - inicioMes).Days + 1)
                .Select(i => inicioMes.AddDays(i))
                .ToList();

            var dic = ocupacion.ToDictionary(x => x.Fecha, x => x.Ocupadas);

            dto.OcupacionMes = dias.Select(d => new OcupacionDiaDTO
            {
                Fecha = d.ToString("dd/MM/yyyy"),
                Ocupadas = dic.TryGetValue(d.ToString("dd/MM/yyyy"), out var v) ? v : 0
            }).ToList();

            // ==========================================================
            // 2) INGRESOS DEL MES (checkouts por día) -> FechaSalidaConfirmacion
            //    Usamos TotalPagado como ingreso total del checkout
            // ==========================================================
            var ingresosRaw = await _dbContext.Recepcions
                .Where(r => r.FechaSalidaConfirmacion.HasValue
                            && r.FechaSalidaConfirmacion.Value.Date >= inicioMes
                            && r.FechaSalidaConfirmacion.Value.Date <= hoy)
                .GroupBy(r => r.FechaSalidaConfirmacion!.Value.Date)
                .Select(g => new IngresoDiaDTO
                {
                    Fecha = g.Key.ToString("dd/MM/yyyy"),
                    FechaDate = g.Key,                // ✅ IMPORTANTÍSIMO
                    Monto = g.Sum(x => x.TotalPagado ?? 0m)
                })
                .ToListAsync();

            var dicIng = ingresosRaw.ToDictionary(x => x.FechaDate.Date, x => x.Monto);

            dto.IngresosMesCheckout = dias.Select(d => new IngresoDiaDTO
            {
                Fecha = d.ToString("dd/MM/yyyy"),
                FechaDate = d,                       // ✅ IMPORTANTÍSIMO
                Monto = dicIng.TryGetValue(d.Date, out var m) ? m : 0m
            }).ToList();

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
            var hoy = DateTime.Today;

            return await _dbContext.Reservas
                .Where(r => r.FechaEntrada.HasValue &&
                            r.FechaEntrada.Value.Date == hoy)
                .CountAsync();
        }

        public async Task<int> TotalReservasMes()
        {
            var hoy = DateTime.Today;

            return await _dbContext.Reservas
                .Where(r => r.FechaEntrada.HasValue &&
                            r.FechaEntrada.Value.Month == hoy.Month &&
                            r.FechaEntrada.Value.Year == hoy.Year)
                .CountAsync();
        }

    }
}
