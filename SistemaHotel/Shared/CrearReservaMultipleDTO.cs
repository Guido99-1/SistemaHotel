using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaHotel.Shared
{
    public class CrearReservaMultipleDTO
    {
        [Required(ErrorMessage = "Cliente es requerido")]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "Fecha de entrada es requerida")]
        public DateTime FechaEntrada { get; set; }

        [Required(ErrorMessage = "Fecha de salida es requerida")]
        public DateTime FechaSalida { get; set; }

        [Required(ErrorMessage = "Monto total es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monto debe ser mayor a 0")]
        public decimal MontoTotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Adelanto no puede ser negativo")]
        public decimal Adelanto { get; set; } = 0;

        [Required(ErrorMessage = "Debe seleccionar al menos una habitación")]
        public List<HabitacionReservaDTO> Habitaciones { get; set; } = new();

        public string? Observacion { get; set; }
    }

    public class HabitacionReservaDTO
    {
        [Required]
        public int IdHabitacion { get; set; }

        [Required(ErrorMessage = "Cantidad de personas es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe ingresar al menos 1 persona")]
        public int CantidadPersonas { get; set; }
    }
}
