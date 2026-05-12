using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHotel.Shared
{
    public class ReservaGrupoDTO
    {
        public int IdReservaGrupo { get; set; }
        public int? IdCliente { get; set; }
        public DateTime? FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public decimal? MontoTotal { get; set; }
        public decimal? PrecioPersona { get; set; }
        public int? TotalPersonas { get; set; }
        public string? Observacion { get; set; }
        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public List<ReservaDetalleDTO> Reservas { get; set; } = new();
    }
}
