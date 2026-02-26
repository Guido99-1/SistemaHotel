using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHotel.Shared
{
    public class ReservaDTO
    {
        public int IdReserva { get; set; }

        public int? IdCliente { get; set; }

        public int? IdHabitacion { get; set; }

        public DateTime? FechaEntrada { get; set; }
        public DateTime? FechaSalidaReserva { get; set; }

        public decimal? PrecioInicial { get; set; }
        public decimal? Adelanto { get; set; }

        public string? Observacion { get; set; }

        public string? Estado { get; set; }
        public string EstadoReserva { get; set; } = "RESERVADA";
        public ClienteDTO? IdClienteNavigation { get; set; }
        public string? Cliente { get; set; }
        public string? Habitacion { get; set; }
        public decimal Total { get; set; }
        public decimal ValorCancelado { get; set; }
        public decimal ValorPendiente { get; set; }


    }
}
