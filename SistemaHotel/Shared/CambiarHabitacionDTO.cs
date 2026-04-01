using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHotel.Shared
{
    public class CambiarHabitacionDTO
    {
        public int IdRecepcion { get; set; }
        public int IdNuevaHabitacion { get; set; }
        public String? Observacion { get; set; }
    }
}
