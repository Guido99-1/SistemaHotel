using SistemaHotel.Client.Servicios.Contratos;
using SistemaHotel.Shared;
using System.Net.Http.Json;

namespace SistemaHotel.Client.Servicios.Implementacion
{
    public class HabitacionServicio : IHabitacionServicio
    {
        private readonly HttpClient _http;
        public HabitacionServicio(HttpClient http)
        {
            _http = http;
        }

        public async Task<ResponseDTO<HabitacionDTO>> Crear(HabitacionDTO entidad)
        {
            var httpResp = await _http.PostAsJsonAsync("api/habitacion/Guardar", entidad);
            return await ReadResponseOrError<ResponseDTO<HabitacionDTO>>(httpResp);
        }

        public async Task<bool> Editar(HabitacionDTO entidad)
        {
            var httpResp = await _http.PutAsJsonAsync("api/habitacion/Editar", entidad);
            var resp = await ReadResponseOrError<ResponseDTO<HabitacionDTO>>(httpResp);
            return resp.status;
        }

        public async Task<bool> Eliminar(int id)
        {
            var httpResp = await _http.DeleteAsync($"api/habitacion/Eliminar/{id}");
            var resp = await ReadResponseOrError<ResponseDTO<string>>(httpResp);
            return resp.status;
        }

        public async Task<ResponseDTO<List<HabitacionDTO>>> Lista()
        {
            // ✅ No usar GetFromJsonAsync porque revienta si el server devuelve 500
            var httpResp = await _http.GetAsync("api/habitacion/Lista");
            return await ReadResponseOrError<ResponseDTO<List<HabitacionDTO>>>(httpResp);
        }

        public async Task<ResponseDTO<HabitacionDTO>> Obtener(int idHabitacion)
        {
            var httpResp = await _http.GetAsync($"api/habitacion/Obtener/{idHabitacion}");
            return await ReadResponseOrError<ResponseDTO<HabitacionDTO>>(httpResp);
        }

        public async Task<ResponseDTO<List<int>>> OcupacionPorRango(string fechaInicio, string fechaFin)
        {
            var url =
                $"api/Habitacion/OcupacionPorRango?fechaInicio={Uri.EscapeDataString(fechaInicio)}&fechaFin={Uri.EscapeDataString(fechaFin)}";

            var result = await _http.GetFromJsonAsync<ResponseDTO<List<int>>>(url);

            return result ?? new ResponseDTO<List<int>>
            {
                status = false,
                msg = "No se pudo leer la respuesta del servidor.",
                value = new List<int>()
            };
        }

        // -------------------------
        // Helper: evita JsonException cuando el server devuelve HTML/Texto
        // -------------------------
        private static async Task<T> ReadResponseOrError<T>(HttpResponseMessage httpResp) where T : class, new()
        {
            var raw = await httpResp.Content.ReadAsStringAsync();

            if (!httpResp.IsSuccessStatusCode)
            {
                // Si T es ResponseDTO<algo>, intentamos rellenar msg
                if (typeof(T).IsGenericType &&
                    typeof(T).GetGenericTypeDefinition() == typeof(ResponseDTO<>))
                {
                    dynamic dto = new T();
                    dto.status = false;
                    dto.msg = raw;
                    dto.value = null;
                    return (T)dto;
                }

                return new T();
            }

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
        public async Task<ResponseDTO<List<HabitacionDTO>>> ListaDisponibles()
        {
            var httpResp = await _http.GetAsync("api/habitacion/Disponibles");
            return await ReadResponseOrError<ResponseDTO<List<HabitacionDTO>>>(httpResp);
        }
    }
}