using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using SistemaHotel.Shared;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SistemaHotel.Client.Utilidades
{
    public class AutenticacionExtension : AuthenticationStateProvider
    {
        private readonly ISessionStorageService _sessionStorage;
        private ClaimsPrincipal _sinInformacion = new ClaimsPrincipal(new ClaimsIdentity());

        public AutenticacionExtension(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task ActualizarEstadoAutenticacion(LoginResponseDTO? loginResponse)
        {
            ClaimsPrincipal claimsPrincipal;

            if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
            {
                await _sessionStorage.SetItemAsStringAsync("jwtToken", loginResponse.Token);

                UsuarioDTO usuario = loginResponse.Usuario;
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.IdRolUsuarioNavigation.Descripcion)
                }, "jwt"));
            }
            else
            {
                claimsPrincipal = _sinInformacion;
                await _sessionStorage.RemoveItemAsync("jwtToken");
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _sessionStorage.GetItemAsStringAsync("jwtToken");

            if (string.IsNullOrEmpty(token))
                return await Task.FromResult(new AuthenticationState(_sinInformacion));

            try
            {
                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var claimPrincipal = new ClaimsPrincipal(identity);
                return await Task.FromResult(new AuthenticationState(claimPrincipal));
            }
            catch
            {
                // Si el token es inválido o expiró, lo eliminamos
                await _sessionStorage.RemoveItemAsync("jwtToken");
                return await Task.FromResult(new AuthenticationState(_sinInformacion));
            }
        }

        public async Task Logout()
        {
            await _sessionStorage.RemoveItemAsync("jwtToken");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_sinInformacion)));
        }

        // ── Decodifica el payload del JWT y extrae los claims ──────────────────
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();

            // El JWT tiene 3 partes: header.payload.signature
            var parts = jwt.Split('.');
            if (parts.Length != 3)
                return claims;

            // Decodificamos el payload (segunda parte)
            var payload = parts[1];

            // Base64Url → Base64 estándar
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "=";  break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

            if (keyValuePairs == null)
                return claims;

            foreach (var kvp in keyValuePairs)
            {
                // Mapeamos las claves estándar de JWT a ClaimTypes
                var type = kvp.Key switch
                {
                    "sub"                                                                               => ClaimTypes.NameIdentifier,
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"             => ClaimTypes.NameIdentifier,
                    "email"                                                                             => ClaimTypes.Email,
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"               => ClaimTypes.Email,
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"                     => ClaimTypes.Role,
                    "role"                                                                              => ClaimTypes.Role,
                    "name"                                                                              => ClaimTypes.Name,
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"                       => ClaimTypes.Name,
                    _                                                                                   => kvp.Key
                };

                var value = kvp.Value.ValueKind == JsonValueKind.String
                    ? kvp.Value.GetString() ?? ""
                    : kvp.Value.ToString();

                claims.Add(new Claim(type, value));
            }

            return claims;
        }
    }
}
