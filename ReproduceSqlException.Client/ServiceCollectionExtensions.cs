using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace ReproduceSqlException.Client
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTestWebApiClient(this IServiceCollection services)
        {
            services
                .AddHttpClient(nameof(TestClient), (_, httpClient) => ConfigureClient(httpClient))
                .ConfigurePrimaryHttpMessageHandler(_ => CreateHttpMessageHandler());

            services.AddSingleton<TestClient>();
        }

        private static void ConfigureClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new("https://localhost/");
            httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));
        }

        private static HttpMessageHandler CreateHttpMessageHandler()
        {
            return new HttpClientHandler
            {
                MaxConnectionsPerServer = 10,
            };
        }
    }
}
