using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHotel.Shared
{
    public class ReservaReporteDTO
    {
        public string NombreCliente { get; set; }
        public string NroHabitacion { get; set; }
        public string FechaEntrada { get; set; }
        public string FechaSalida { get; set; }
        public decimal Total { get; set; }
        public decimal ValorCancelado { get; set; }
        public decimal ValorPendiente { get; set; }
        public string EstadoReserva { get; set; }

    }
}
