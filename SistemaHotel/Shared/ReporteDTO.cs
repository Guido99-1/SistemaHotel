using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaHotel.Shared
{
    public class ReporteDTO
    {
        public string? NombreCliente { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NroDocumento { get; set; }
        public string? NroHabitacion { get; set; }
        public string? FechaEntrada { get; set; }
        public string? FechaSalida { get; set; }
        public decimal? Adelanto { get; set; }
        public decimal? PrecioRestante { get; set; }
        public string? MetodoPago { get; set; } = "";
        public string? NotaMetodoPago { get; set; }  // Detalle del método de pago (especialmente útil para pagos MIXTOS)
        public decimal? CostoPenalidad { get; set; }
        public decimal? TotalPagado { get; set; }
        public string? Observacion { get; set; }

        /// <summary>
        /// Devuelve el método de pago formateado para reportes.
        /// Si es MIXTO, muestra el desglose completo. Si es OTRO, muestra el detalle.
        /// </summary>
        public string MetodoPagoDetallado
        {
            get
            {
                if (string.IsNullOrEmpty(MetodoPago))
                    return "";

                if (MetodoPago == "MIXTO" && !string.IsNullOrEmpty(NotaMetodoPago))
                    return NotaMetodoPago;

                if (MetodoPago == "OTRO" && !string.IsNullOrEmpty(NotaMetodoPago))
                    return $"OTRO ({NotaMetodoPago})";

                return MetodoPago;
            }
        }
    }
}
