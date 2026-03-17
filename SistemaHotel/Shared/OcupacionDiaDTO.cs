using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHotel.Shared
{
    public class OcupacionDiaDTO
    {
        public string Fecha { get; set; } = "";
        public DateTime FechaDate { get; set; }   // ✅ clave para ordenar y graficar
        public int Ocupadas { get; set; }
    }
}
