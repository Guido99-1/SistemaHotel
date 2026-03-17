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
            var httpResp = await _http.PutAsJsonAsync($"api/Reservas/CambiarEstado/{idReserva}", estadoReserva);
            return await ReadResponseOrError<ResponseDTO<bool>>(httpResp);
        }

        public async Task<ResponseDTO<bool>> Cancelar(int idReserva)
        {
            var httpResp = await _http.PutAsync($"api/Reservas/Cancelar/{idReserva}", null);
            return await ReadResponseOrError<ResponseDTO<bool>>(httpResp);
        }

        public async Task<ResponseDTO<ReservaDTO>> ObtenerPorId(int idReserva)
        {
            var httpResp = await _http.GetAsync($"api/Reservas/ObtenerPorId/{idReserva}");
            return await ReadResponseOrError<ResponseDTO<ReservaDTO>>(httpResp);
        }

        public async Task<ResponseDTO<ReservaDTO>> EditarReserva(int idReserva, ReservaDTO request)
        {
            var httpResp = await _http.PutAsJsonAsync($"api/Reservas/Editar/{idReserva}", request);
            return await ReadResponseOrError<ResponseDTO<ReservaDTO>>(httpResp);
        }

        public async Task<ResponseDTO<bool>> Eliminar(int idReserva)
        {
            var httpResp = await _http.DeleteAsync($"api/Reservas/Eliminar/{idReserva}");
            return await ReadResponseOrError<ResponseDTO<bool>>(httpResp);
        }

        public async Task<byte[]> ExportarPdf(string fechaInicio, string fechaFin)
        {
            return await _http.GetByteArrayAsync($"api/Reservas/ExportarPdf?fechaInicio={fechaInicio}&fechaFin={fechaFin}");
        }

        // -------------------------
        // Helper: evita JsonException cuando el server devuelve HTML/Texto
        // -------------------------
        private static async Task<T> ReadResponseOrError<T>(HttpResponseMessage httpResp)
    where T : class, new()
        {
            // Caso OK: parse normal
            if (httpResp.IsSuccessStatusCode)
            {
                try
                {
                    var ok = await httpResp.Content.ReadFromJsonAsync<T>();
                    return ok ?? new T();
                }
                catch
                {
                    return new T();
                }
            }

            // Caso ERROR: intentamos parsear ResponseDTO<algo> y sacar msg
            try
            {
                var err = await httpResp.Content.ReadFromJsonAsync<T>();
                if (err is not null)
                    return err;
            }
            catch
            {
                // si no se pudo parsear, seguimos
            }

            // Fallback: devolvemos un dto con msg "limpio"
            var raw = await httpResp.Content.ReadAsStringAsync();

            if (typeof(T).IsGenericType &&
                typeof(T).GetGenericTypeDefinition() == typeof(ResponseDTO<>))
            {
                dynamic dto = new T();
                dto.status = false;
                dto.value = null;

                // si el server devolvió JSON con ResponseDTO, raw puede traer eso,
                // pero como no se pudo parsear, dejamos un texto genérico:
                dto.msg = string.IsNullOrWhiteSpace(raw)
                    ? $"Error HTTP {(int)httpResp.StatusCode} - {httpResp.ReasonPhrase}"
                    : $"Error HTTP {(int)httpResp.StatusCode} - {httpResp.ReasonPhrase}";

                return (T)dto;
            }

            return new T();
        }

    }
}