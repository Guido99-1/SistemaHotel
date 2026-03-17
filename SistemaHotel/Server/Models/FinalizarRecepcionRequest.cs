namespace SistemaHotel.Server.Models
{
    public class FinalizarRecepcionRequest
    {
        public DateTime fechaSalidaConfirmacion { get; set; }
        public decimal costoPenalidad { get; set; }
    }
}
