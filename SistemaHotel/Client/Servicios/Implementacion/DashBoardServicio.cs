using SistemaHotel.Client.Servicios.Contratos;
using SistemaHotel.Shared;
using System.Net.Http.Json;

namespace SistemaHotel.Client.Servicios.Implementacion
{
    public class DashBoardServicio : IDashBoardServicio
    {
        private readonly HttpClient _http;

        public DashBoardServicio(HttpClient http)
        {
            _http = http;
        }

        // -----------------------------
        // Resumen (ResponseDTO<DashBoardDTO>)
        // -----------------------------
        public async Task<ResponseDTO<DashBoardDTO>> Resumen()
        {
            // OJO: misma ruta que el Controller -> api/DashBoard/Resumen
            var result = await _http.GetFromJsonAsync<ResponseDTO<DashBoardDTO>>("api/DashBoard/Resumen");
            return result!;
        }

        // -----------------------------
        // Reservas (int)
        // -----------------------------
        public Task<int> TotalReservasHoy()
            => GetIntSafe("api/DashBoard/TotalReservasHoy");

        public Task<int> TotalReservasMes()
            => GetIntSafe("api/DashBoard/TotalReservasMes");

        // -----------------------------
        // Habitaciones (int)
        // -----------------------------
        public Task<int> TotalHabitaciones()
            => GetIntSafe("api/DashBoard/TotalHabitaciones");

        public Task<int> TotalHabitacionesDisponibles()
            => GetIntSafe("api/DashBoard/TotalHabitacionesDisponibles");

        public Task<int> TotalHabitacionesOcupadas()
            => GetIntSafe("api/DashBoard/TotalHabitacionesOcupadas");

        public Task<int> TotalHabitacionesEnLimpieza()
            => GetIntSafe("api/DashBoard/TotalHabitacionesEnLimpieza");

        // -----------------------------
        // Helper: evita que reviente si la API responde error/HTML
        // -----------------------------
        private async Task<int> GetIntSafe(string url)
        {
            var resp = await _http.GetAsync(url);

            if (!resp.IsSuccessStatusCode)
                return 0;

            var text = await resp.Content.ReadAsStringAsync();

            // Si vino HTML, no intentes parsear JSON
            if (!string.IsNullOrWhiteSpace(text) && text.TrimStart().StartsWith("<"))
                return 0;

            // Puede venir "5" (texto plano) o JSON 5
            if (int.TryParse(text, out var n))
                return n;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<int>(text);
            }
            catch
            {
                return 0;
            }
        }

    }
}
