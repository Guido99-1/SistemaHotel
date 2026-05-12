using System;
using System.Collections.Generic;

namespace SistemaHotel.Server.Models;

public partial class ReservaGrupo
{
    public int IdReservaGrupo { get; set; }

    public int? IdCliente { get; set; }

    public DateTime? FechaEntrada { get; set; }

    public DateTime? FechaSalida { get; set; }

    public decimal? MontoTotal { get; set; }

    public decimal? PrecioPersona { get; set; }

    public int? TotalPersonas { get; set; }

    public string? Observacion { get; set; }

    public bool? Estado { get; set; } = true;

    // FechaCreacion se establece desde el servicio FechaService o por getdate() en BD
    public DateTime? FechaCreacion { get; set; }

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
