using Blazored.SessionStorage;

namespace SistemaHotel.Client.Utilidades
{
    public class HttpClientInterceptor : DelegatingHandler
    {
        private readonly ISessionStorageService _sessionStorage;

        public HttpClientInterceptor(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _sessionStorage.GetItemAsStringAsync("jwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
