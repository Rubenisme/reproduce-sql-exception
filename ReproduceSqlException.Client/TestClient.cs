using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ReproduceSqlException.Client
{
    public class TestClient : WebApiBaseClient
    {
        public TestClient(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }
        
        public Task<string> TestMethod(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/values");
            return Call(request, cancellationToken);
        }
    }
}
