using Microsoft.JSInterop;
using SistemaHotel.Shared;

namespace SistemaHotel.Client.Servicios.Implementacion
{
    /// <summary>
    /// Servicio cliente para generar reportes PDF usando jsPDF (JavaScript).
    /// Plantilla unificada con branding del hotel.
    /// </summary>
    public class ReportePdfService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ConfiguracionHotelDTO _config;

        public ReportePdfService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _config = new ConfiguracionHotelDTO();
        }

        /// <summary>
        /// Genera un reporte PDF con plantilla unificada del hotel.
        /// </summary>
        public async Task GenerarReporte(
            string titulo,
            string subtitulo,
            List<string> encabezados,
            List<object[]> datos,
            string usuarioGenerador,
            string nombreArchivo,
            List<TotalReporte>? totales = null,
            string orientacion = "landscape")
        {
            // Construir array de totales (siempre del mismo tipo, evita problemas de inferencia)
            var totalesArray = (totales ?? new List<TotalReporte>())
                .Select(t => new { label = t.Etiqueta, valor = t.Valor })
                .ToArray();

            // Construir matriz de datos convertidos a string
            var datosConvertidos = datos
                .Select(row => row.Select(cell => ConvertirCelda(cell)).ToArray())
                .ToArray();

            var configuracion = new
            {
                nombreHotel = _config.NombreHotel,
                ruc = _config.Ruc,
                direccion = _config.Direccion,
                telefono = _config.Telefono,
                email = _config.Email,
                colorPrimario = _config.ColorPrimario,
                titulo = titulo,
                subtitulo = subtitulo,
                usuarioGenerador = usuarioGenerador,
                fechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                encabezados = encabezados,
                datos = datosConvertidos,
                totales = totalesArray,
                nombreArchivo = nombreArchivo,
                orientacion = orientacion
            };

            await _jsRuntime.InvokeVoidAsync("GenerarReportePdf", configuracion);
        }

        /// <summary>
        /// Convierte un valor de celda a string para el PDF.
        /// Garantiza que el retorno siempre sea string (no nullable).
        /// </summary>
        private static string ConvertirCelda(object? cell)
        {
            if (cell == null) return string.Empty;
            if (cell is decimal d) return d.ToString("N2");
            if (cell is double db) return db.ToString("N2");
            if (cell is DateTime dt) return dt.ToString("dd/MM/yyyy");
            return cell.ToString() ?? string.Empty;
        }
    }

    public class TotalReporte
    {
        public string Etiqueta { get; set; } = "";
        public decimal Valor { get; set; }
    }
}
