using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaHotel.Server.Models

{
    public class Reserva
    {
        [Key]
        public int IdReserva { get; set; }

        public int? IdCliente { get; set; }
        public int? IdHabitacion { get; set; }

        public DateTime? FechaEntrada { get; set; }
        public DateTime? FechaSalidaReserva { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecioInicial { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Adelanto { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecioRestante { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalPagado { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CostoPenalidad { get; set; }

        public string? Observacion { get; set; }

        public bool Estado { get; set; }
        public string EstadoReserva { get; set; } = null!;


        // -----------------------
        // Navigation
        // -----------------------


        [ForeignKey(nameof(IdCliente))]
        public virtual Cliente IdClienteNavigation { get; set; }

        [ForeignKey(nameof(IdHabitacion))]
        public virtual Habitacion IdHabitacionNavigation { get; set; }
        public virtual ICollection<Recepcion> Recepcions { get; set; } = new List<Recepcion>();
    }
}
