using System;
using System.Collections.Generic;

namespace SistemaHotel.Shared
{
    public class DashBoardDTO
    {
        // ═══════════════════════════════════════════════════
        // KPIS PRINCIPALES
        // ═══════════════════════════════════════════════════
        public int TotalHabitaciones { get; set; }
        public int TotalHabitacionesDisponibles { get; set; }
        public int TotalHabitacionesOcupadas { get; set; }
        public int TotalHabitacionesEnLimpieza { get; set; }

        // Reservas
        public int TotalReservasHoy { get; set; }
        public int TotalReservasMes { get; set; }
        public int ReservasPendientesHoy { get; set; }     // No han hecho check-in aún
        public int CheckInsHoy { get; set; }                // Ya hicieron check-in hoy
        public int CheckOutsHoy { get; set; }               // Ya hicieron check-out hoy

        // Clientes
        public int TotalClientes { get; set; }
        public int ClientesNuevosMes { get; set; }

        // ═══════════════════════════════════════════════════
        // INGRESOS Y FINANZAS
        // ═══════════════════════════════════════════════════
        public decimal IngresosHoy { get; set; }
        public decimal IngresosMes { get; set; }
        public decimal IngresosMesAnterior { get; set; }
        public decimal IngresosAnio { get; set; }
        public decimal AdelantosPendientes { get; set; }    // Adelantos en reservas activas
        public decimal PromedioIngresoDia { get; set; }
        public decimal PromedioPorReserva { get; set; }
        public decimal PenalidadesMes { get; set; }

        // ═══════════════════════════════════════════════════
        // MÉTRICAS DE OCUPACIÓN
        // ═══════════════════════════════════════════════════
        public double TasaOcupacionActual { get; set; }
        public double TasaOcupacionPromedioMes { get; set; }
        public int NochesOcupadasMes { get; set; }

        // ═══════════════════════════════════════════════════
        // SERIES DE TIEMPO (gráficas)
        // ═══════════════════════════════════════════════════
        public List<OcupacionDiaDTO> OcupacionMes { get; set; } = new();
        public List<IngresoDiaDTO> IngresosMesCheckout { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // ANÁLISIS POR CATEGORÍA Y PISO
        // ═══════════════════════════════════════════════════
        public List<OcupacionPorCategoriaDTO> OcupacionPorCategoria { get; set; } = new();
        public List<OcupacionPorPisoDTO> OcupacionPorPiso { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // MÉTODOS DE PAGO
        // ═══════════════════════════════════════════════════
        public List<MetodoPagoDTO> DistribucionMetodosPago { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // TOP / RANKINGS
        // ═══════════════════════════════════════════════════
        public List<TopClienteDTO> TopClientes { get; set; } = new();
        public List<TopHabitacionDTO> TopHabitaciones { get; set; } = new();

        // ═══════════════════════════════════════════════════
        // PRÓXIMOS EVENTOS
        // ═══════════════════════════════════════════════════
        public List<ReservaProximaDTO> ProximosCheckIns { get; set; } = new();

        // Estado de reservas (distribución)
        public int ReservasReservadas { get; set; }
        public int ReservasConfirmadas { get; set; }
        public int ReservasFinalizadas { get; set; }
        public int ReservasCanceladas { get; set; }

        // ═══════════════════════════════════════════════════
        // RESERVAS DE GERENCIA
        // ═══════════════════════════════════════════════════
        public int TotalReservasGerenciaMes { get; set; }
        public int TotalReservasGerenciaAnio { get; set; }
        public decimal IngresosGerenciaMes { get; set; }
        public decimal IngresosGerenciaAnio { get; set; }
        public decimal IngresosGerenciaMesAnterior { get; set; }
        public int ReservasGerenciaFestivo { get; set; }
        public int ReservasGerenciaNormal { get; set; }
        public List<IngresoDiaDTO> IngresosGerenciaDiarios { get; set; } = new();
        public List<ReservaProximaDTO> ReservasGerenciaRecientes { get; set; } = new();
    }

    // ═══════════════════════════════════════════════════
    // DTOs auxiliares para análisis
    // ═══════════════════════════════════════════════════
    public class OcupacionPorCategoriaDTO
    {
        public string Categoria { get; set; } = "";
        public int Total { get; set; }
        public int Ocupadas { get; set; }
        public int Disponibles { get; set; }
        public decimal IngresoMes { get; set; }
        public double Porcentaje { get; set; }
    }

    public class OcupacionPorPisoDTO
    {
        public string Piso { get; set; } = "";
        public int TotalHabitaciones { get; set; }
        public int HabitacionesOcupadas { get; set; }
        public double TasaOcupacion { get; set; }
    }

    public class MetodoPagoDTO
    {
        public string Metodo { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal MontoTotal { get; set; }
        public double Porcentaje { get; set; }
    }

    public class TopClienteDTO
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = "";
        public int CantidadReservas { get; set; }
        public decimal TotalGastado { get; set; }
    }

    public class TopHabitacionDTO
    {
        public int IdHabitacion { get; set; }
        public string Numero { get; set; } = "";
        public string Categoria { get; set; } = "";
        public int VecesOcupada { get; set; }
        public decimal IngresoTotal { get; set; }
    }

    public class ReservaProximaDTO
    {
        public int IdReserva { get; set; }
        public string Cliente { get; set; } = "";
        public string Habitacion { get; set; } = "";
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaSalida { get; set; }
        public string Estado { get; set; } = "";
        public decimal Total { get; set; }
    }
}
