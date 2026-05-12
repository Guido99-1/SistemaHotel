using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaHotel.Server.Utilidades;

namespace SistemaHotel.Server.Controllers
{
    /// <summary>
    /// Controlador para diagnosticar el estado del sistema, especialmente fechas.
    /// Útil después de desplegar a producción para verificar que todo funciona correctamente.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticoController : ControllerBase
    {
        private readonly IFechaService _fechaService;

        public DiagnosticoController(IFechaService fechaService)
        {
            _fechaService = fechaService;
        }

        /// <summary>
        /// Endpoint público para verificar el manejo de fechas en el servidor.
        /// Acceder a: GET /api/diagnostico/fechas
        /// </summary>
        [HttpGet("fechas")]
        [AllowAnonymous]
        public IActionResult VerificarFechas()
        {
            var ahoraServidor = DateTime.Now;
            var ahoraUtc = DateTime.UtcNow;
            var ahoraServicio = _fechaService.Now;
            var hoyServicio = _fechaService.Today;

            // Diferencia entre la hora del servidor y la zona configurada
            var diferenciaHoras = (ahoraServicio - ahoraServidor).TotalHours;

            return Ok(new
            {
                EstadoSistema = "✅ Sistema funcionando",
                ZonaHorariaConfigurada = new
                {
                    Id = _fechaService.ZonaHoraria.Id,
                    Nombre = _fechaService.ZonaHoraria.DisplayName,
                    OffsetUtc = _fechaService.ZonaHoraria.BaseUtcOffset.ToString()
                },
                Fechas = new
                {
                    HoraServidor_DateTimeNow = ahoraServidor.ToString("yyyy-MM-dd HH:mm:ss"),
                    HoraUtc_DateTimeUtcNow = ahoraUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                    HoraServicio_FechaServiceNow = ahoraServicio.ToString("yyyy-MM-dd HH:mm:ss"),
                    HoyServicio_FechaServiceToday = hoyServicio.ToString("yyyy-MM-dd HH:mm:ss"),
                    DiferenciaServidorVsServicio_Horas = diferenciaHoras
                },
                Diagnostico = new
                {
                    ServidorEnZonaCorrecta = Math.Abs(diferenciaHoras) < 0.1,
                    Recomendacion = Math.Abs(diferenciaHoras) < 0.1
                        ? "✅ El servidor está en la zona horaria correcta. No hay problemas."
                        : $"⚠️ El servidor está en zona diferente. Hay {diferenciaHoras:F1}h de diferencia. Use SIEMPRE el FechaService en el código."
                },
                InstruccionesUso = new[]
                {
                    "Para fechas en el SERVIDOR: usa _fechaService.Now y _fechaService.Today",
                    "Para fechas en el CLIENTE Blazor WASM: DateTime.Now/Today está bien (usa zona del navegador)",
                    "Para tokens JWT: usar DateTime.UtcNow (ya configurado en JwtService)"
                }
            });
        }
    }
}
