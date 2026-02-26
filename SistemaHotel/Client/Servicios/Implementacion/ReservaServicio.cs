using SistemaHotel.Client.Servicios.Contratos;
using SistemaHotel.Shared;
using System.Net.Http;
using System.Net.Http.Json;

namespace SistemaHotel.Client.Servicios.Implementacion
{
    public class ReservaServicio : IReservaServicio
    {
        private readonly HttpClient _http;

        public ReservaServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<ResponseDTO<ReservaDTO>> Crear(ReservaDTO entidad)
        {
            var httpResp = await _http.PostAsJsonAsync("api/Reservas/Guardar", entidad);
            return await ReadResponseOrError<ResponseDTO<ReservaDTO>>(httpResp);
        }

        public async Task<bool> Editar(ReservaDTO entidad)
        {
            var httpResp = await _http.PutAsJsonAsync("api/Reservas/Editar", entidad);
            var dto = await ReadResponseOrError<ResponseDTO<ReservaDTO>>(httpResp);
            return dto.status;
        }

        public async Task<ResponseDTO<ReservaDTO>> Obtener(int idHabitacion)
        {
            var httpResp = await _http.GetAsync($"api/Reservas/Obtener/{idHabitacion}");
            return await ReadResponseOrError<ResponseDTO<ReservaDTO>>(httpResp);
        }

        public async Task<ResponseDTO<List<ReservaDTO>>> Reporte(string fechaInicio, string fechaFin)
        {
            var httpResp = await _http.GetAsync($"api/Reservas/Reporte?fechaInicio={fechaInicio}&fechaFin={fechaFin}");
            return await ReadResponseOrError<ResponseDTO<List<ReservaDTO>>>(httpResp);
        }

        public async Task<ResponseDTO<List<ReservaDTO>>> Filtrar(string fechaInicio, string fechaFin)
        {
            var httpResp = await _http.GetAsync($"api/Reservas/Filtrar?fechaInicio={fechaInicio}&fechaFin={fechaFin}");
            return await ReadResponseOrError<ResponseDTO<List<ReservaDTO>>>(httpResp);
        }

        public async Task<ResponseDTO<List<ReservaReporteDTO>>> FiltrarListado(string fechaInicio, string fechaFin)
        {
            var httpResp = await _http.GetAsync($"api/Reservas/FiltrarListado?fechaInicio={fechaInicio}&fechaFin={fechaFin}");
            return await ReadResponseOrError<ResponseDTO<List<ReservaReporteDTO>>>(httpResp);
        }

        public async Task<ResponseDTO<bool>> CambiarEstado(int idReserva, string estadoReserva)
        {
            // OJO: si tu API espera string simple, esto está OK.
            // Si espera un objeto, se cambia aquí.
            var httpResp = await _http.PutAsJsonAsync($"api/Reservas/CambiarEstado/{idReserva}", estadoReserva);
            return await ReadResponseOrError<ResponseDTO<bool>>(httpResp);
        }

        public async Task<ResponseDTO<bool>> Cancelar(int idReserva)
        {
            var httpResp = await _http.PutAsync($"api/Reservas/Cancelar/{idReserva}", null);
            return await ReadResponseOrError<ResponseDTO<bool>>(httpResp);
        }

        public async Task<byte[]> ExportarPdf(string fechaInicio, string fechaFin)
        {
            // Aquí sí debe reventar si falla, porque necesitas el PDF.
            return await _http.GetByteArrayAsync($"api/Reservas/ExportarPdf?fechaInicio={fechaInicio}&fechaFin={fechaFin}");
        }

        // -------------------------
        // Helper: evita JsonException cuando el server devuelve HTML/Texto
        // -------------------------
        private static async Task<T> ReadResponseOrError<T>(HttpResponseMessage httpResp) where T : class, new()
        {
            var raw = await httpResp.Content.ReadAsStringAsync();

            // Si no fue 2xx, intentamos devolver ResponseDTO con msg
            if (!httpResp.IsSuccessStatusCode)
            {
                if (typeof(T).IsGenericType &&
                    typeof(T).GetGenericTypeDefinition() == typeof(ResponseDTO<>))
                {
                    dynamic dto = new T();
                    dto.status = false;
                    dto.msg = string.IsNullOrWhiteSpace(raw) ? "Error del servidor." : raw;
                    dto.value = null;
                    return (T)dto;
                }

                return new T();
            }

            // Si fue OK, intentamos parsear JSON
            try
            {
                var obj = await httpResp.Content.ReadFromJsonAsync<T>();
                return obj ?? new T();
            }
            catch
            {
                return new T();
            }
        }
    }
}