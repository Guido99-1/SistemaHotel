namespace SistemaHotel.Shared
{
    /// <summary>
    /// Configuración del hotel para mostrar en reportes (branding)
    /// </summary>
    public class ConfiguracionHotelDTO
    {
        public string NombreHotel { get; set; } = "HOSTERÍA AGOYÁN";
        public string Ruc { get; set; } = "1804742532001";
        public string Direccion { get; set; } = "Av. Principal s/n, Baños - Tungurahua";
        public string Telefono { get; set; } = "0962213000";
        public string Email { get; set; } = "agoyanhosteria@gmail.com";
        public string Website { get; set; } = "www.hosteriaagoyan.com";

        // Colores de marca (corporativos)
        public string ColorPrimario { get; set; } = "#1976D2";  // Azul
        public string ColorSecundario { get; set; } = "#388E3C"; // Verde
        public string ColorEncabezado { get; set; } = "#1976D2";
        public string ColorTotales { get; set; } = "#E3F2FD";
    }
}
