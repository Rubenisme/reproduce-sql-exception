using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ReproduceSqlException.Client
{
    public class WebApiBaseClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        protected WebApiBaseClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            return _httpClientFactory.CreateClient(nameof(TestClient));
        }

        protected async Task<string> Call(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var client = CreateClient();
            using var response = await client.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            return response.IsSuccessStatusCode
                ? content
                : throw new($"Remote API returned {response.StatusCode} for {request.RequestUri} and content {content}");
        }
    }
}
