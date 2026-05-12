using System;
using System.Collections.Generic;

namespace SistemaHotel.Shared
{
    public class ReservaMultipleResponseDTO
    {
        public int IdReservaGrupo { get; set; }
        public int IdCliente { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaSalida { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PrecioPersona { get; set; }
        public int TotalPersonas { get; set; }
        public string? Observacion { get; set; }
        public List<ReservaDetalleDTO> Reservas { get; set; } = new();
    }

    public class ReservaDetalleDTO
    {
        public int IdReserva { get; set; }
        public int IdHabitacion { get; set; }
        public string? NumeroHabitacion { get; set; }
        public int CantidadPersonas { get; set; }
        public decimal PrecioCalculado { get; set; }
    }
}
