using SistemaHotel.Client.Servicios.Contratos;
using SistemaHotel.Shared;
using System.Net.Http.Json;

namespace SistemaHotel.Client.Servicios.Implementacion
{
    public class ReservaGrupoServicio : IReservaGrupoServicio
    {
        private readonly HttpClient _http;

        public ReservaGrupoServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<ResponseDTO<ReservaMultipleResponseDTO>> CrearReservaMultiple(CrearReservaMultipleDTO request)
        {
            var httpResp = await _http.PostAsJsonAsync("api/ReservaGrupos/CrearMultiple", request);
            return await ReadResponseOrError<ResponseDTO<ReservaMultipleResponseDTO>>(httpResp);
        }

        public async Task<ResponseDTO<ReservaGrupoDTO>> ObtenerGrupo(int idReservaGrupo)
        {
            var httpResp = await _http.GetAsync($"api/ReservaGrupos/Obtener/{idReservaGrupo}");
            return await ReadResponseOrError<ResponseDTO<ReservaGrupoDTO>>(httpResp);
        }

        public async Task<ResponseDTO<List<ReservaGrupoDTO>>> ObtenerPorCliente(int idCliente)
        {
            var httpResp = await _http.GetAsync($"api/ReservaGrupos/ObtenerPorCliente/{idCliente}");
            return await ReadResponseOrError<ResponseDTO<List<ReservaGrupoDTO>>>(httpResp);
        }

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
                dto.msg = string.IsNullOrWhiteSpace(raw)
                    ? $"Error HTTP {(int)httpResp.StatusCode} - {httpResp.ReasonPhrase}"
                    : $"Error HTTP {(int)httpResp.StatusCode} - {httpResp.ReasonPhrase}";

                return (T)dto;
            }

            return new T();
        }
    }
}
