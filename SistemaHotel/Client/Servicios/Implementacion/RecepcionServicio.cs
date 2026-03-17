using SistemaHotel.Client.Servicios.Contratos;
using SistemaHotel.Shared;
using System.Net.Http.Json;
using System.Text.Json;

namespace SistemaHotel.Client.Servicios.Implementacion
{
    public class RecepcionServicio : IRecepcionServicio
    {
        private readonly HttpClient _http;

        public RecepcionServicio(HttpClient http)
        {
            _http = http;
        }
        //public async Task<ResponseDTO<bool>> Finalizar(int idRecepcion)
        //{
        //    var httpResp = await _http.PutAsync($"api/Recepcion/Finalizar/{idRecepcion}", null);
        //    return await ReadResponseOrError<ResponseDTO<bool>>(httpResp);
        //}
        public async Task<bool> Finalizar(int idRecepcion, DateTime fechaSalidaConfirmacion, decimal costoPenalidad)
        {
            var response = await _http.PutAsJsonAsync(
                $"api/Recepcion/Finalizar/{idRecepcion}",
                new
                {
                    fechaSalidaConfirmacion,
                    costoPenalidad
                });

            return response.IsSuccessStatusCode;
        }

        public async Task<ResponseDTO<RecepcionDTO>> Crear(RecepcionDTO entidad)
        {
            var httpResp = await _http.PostAsJsonAsync("api/Recepcion/Guardar", entidad);
            return await ReadResponseOrError<ResponseDTO<RecepcionDTO>>(httpResp);
        }

        public async Task<bool> Editar(RecepcionDTO entidad)
        {
            var httpResp = await _http.PutAsJsonAsync("api/Recepcion/Editar", entidad);
            var resp = await ReadResponseOrError<ResponseDTO<RecepcionDTO>>(httpResp);
            return resp.status;
        }

        public async Task<ResponseDTO<RecepcionDTO>> Obtener(int idHabitacion)
        {
            var httpResp = await _http.GetAsync($"api/Recepcion/Obtener/{idHabitacion}");
            return await ReadResponseOrError<ResponseDTO<RecepcionDTO>>(httpResp);
        }

        public async Task<ResponseDTO<List<ReporteDTO>>> Reporte(string fechaInicio, string fechaFin)
        {
            var httpResp = await _http.GetAsync($"api/Recepcion/Reporte?fechaInicio={fechaInicio}&fechaFin={fechaFin}");
            return await ReadResponseOrError<ResponseDTO<List<ReporteDTO>>>(httpResp);
        }


        // -------------------------
        // Helper: evita JsonException cuando el server devuelve HTML/Texto
        // -------------------------
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static async Task<T> ReadResponseOrError<T>(HttpResponseMessage httpResp)
    where T : class, new()
        {
            var raw = await httpResp.Content.ReadAsStringAsync();

            // Si NO es éxito, intenta leer ResponseDTO desde JSON.
            if (!httpResp.IsSuccessStatusCode)
            {
                // 1) si el server devolvió JSON ResponseDTO<...>
                try
                {
                    var dto = JsonSerializer.Deserialize<T>(raw, _jsonOptions);
                    if (dto != null) return dto;
                }
                catch { /* ignore */ }

                // 2) si devolvió HTML u otro texto (ej: error 500 con página HTML)
                if (raw.TrimStart().StartsWith("<"))
                    raw = "Ocurrió un error en el servidor. Revisa la API (500) y el log del Server.";

                // 3) si T es ResponseDTO<>, rellena msg sin romper
                if (typeof(T).IsGenericType &&
                    typeof(T).GetGenericTypeDefinition() == typeof(ResponseDTO<>))
                {
                    dynamic dto = new T();
                    dto.status = false;
                    dto.msg = raw;

                    // OJO: value podría ser bool (no nullable). Si es así, null puede dar problemas.
                    // Mejor: asignar default del tipo genérico.
                    var innerType = typeof(T).GetGenericArguments()[0];
                    dto.value = innerType.IsValueType ? Activator.CreateInstance(innerType) : null;

                    return (T)dto;
                }

                return new T();
            }

            // ✅ Éxito: deserializa desde raw (NO vuelvas a leer el stream)
            try
            {
                var obj = JsonSerializer.Deserialize<T>(raw, _jsonOptions);
                return obj ?? new T();
            }
            catch
            {
                // Si por alguna razón no vino JSON válido
                return new T();
            }
        }
    }
}
