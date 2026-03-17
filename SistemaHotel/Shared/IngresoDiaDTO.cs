using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHotel.Shared
{
    public class IngresoDiaDTO
    {
        public string Fecha { get; set; } = "";
        public DateTime FechaDate { get; set; }
        public decimal Monto { get; set; }
    }
}
