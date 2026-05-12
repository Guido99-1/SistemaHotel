namespace SistemaHotel.Server.Utilidades
{
    /// <summary>
    /// Servicio centralizado para manejo de fechas con zona horaria configurable.
    /// IMPORTANTE: Usar siempre este servicio en lugar de DateTime.Now o DateTime.Today
    /// para evitar problemas cuando el sistema se despliega en servidores con
    /// diferentes zonas horarias (UTC, etc.)
    /// </summary>
    public interface IFechaService
    {
        /// <summary>Hora actual en la zona horaria configurada (Ecuador UTC-5 por defecto)</summary>
        DateTime Now { get; }

        /// <summary>Fecha actual (solo día, sin hora) en la zona horaria configurada</summary>
        DateTime Today { get; }

        /// <summary>Convierte una fecha UTC a la zona horaria configurada</summary>
        DateTime ConvertirDesdeUtc(DateTime fechaUtc);

        /// <summary>Convierte una fecha local a UTC</summary>
        DateTime ConvertirAUtc(DateTime fechaLocal);

        /// <summary>Información de la zona horaria configurada</summary>
        TimeZoneInfo ZonaHoraria { get; }
    }

    public class FechaService : IFechaService
    {
        private readonly TimeZoneInfo _zonaHoraria;

        public FechaService(IConfiguration configuration)
        {
            // Lee la zona horaria desde appsettings.json, por defecto Ecuador
            var timeZoneId = configuration["TimeZone:Id"] ?? "SA Pacific Standard Time";

            try
            {
                _zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback: si no encuentra la zona, usa UTC-5 manual
                _zonaHoraria = TimeZoneInfo.CreateCustomTimeZone(
                    "Ecuador",
                    TimeSpan.FromHours(-5),
                    "Ecuador (UTC-5)",
                    "Ecuador (UTC-5)"
                );
            }
        }

        public TimeZoneInfo ZonaHoraria => _zonaHoraria;

        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zonaHoraria);

        public DateTime Today => Now.Date;

        public DateTime ConvertirDesdeUtc(DateTime fechaUtc)
        {
            // Si la fecha no es UTC, asumimos que ya está en la zona local
            if (fechaUtc.Kind == DateTimeKind.Local)
                return fechaUtc;

            var utc = fechaUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc)
                : fechaUtc;

            return TimeZoneInfo.ConvertTimeFromUtc(utc, _zonaHoraria);
        }

        public DateTime ConvertirAUtc(DateTime fechaLocal)
        {
            if (fechaLocal.Kind == DateTimeKind.Utc)
                return fechaLocal;

            var local = fechaLocal.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified)
                : fechaLocal;

            return TimeZoneInfo.ConvertTimeToUtc(local, _zonaHoraria);
        }
    }
}
