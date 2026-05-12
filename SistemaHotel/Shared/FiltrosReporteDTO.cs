namespace SistemaHotel.Shared
{
    /// <summary>
    /// Filtros para el reporte de Recepciones
    /// </summary>
    public class FiltroReporteRecepcionDTO
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? NombreCliente { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroHabitacion { get; set; }
        public string? MetodoPago { get; set; }
        public bool? SoloConPenalidad { get; set; }
    }

    /// <summary>
    /// Filtros para el reporte de Clientes
    /// </summary>
    public class FiltroReporteClienteDTO
    {
        public string? TipoDocumento { get; set; }
        public string? Texto { get; set; }
        public bool? SoloConReservas { get; set; }
    }

    /// <summary>
    /// Filtros para el reporte de Reservas
    /// </summary>
    public class FiltroReporteReservaDTO
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? NombreCliente { get; set; }
        public string? NumeroHabitacion { get; set; }
        public string? EstadoReserva { get; set; }
        public bool? ConSaldoPendiente { get; set; }
    }
}
