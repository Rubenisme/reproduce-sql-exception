using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ReproduceSqlException.Client;
using ReproduceSqlException.Db;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReproduceSqlException.Tests
{
    [TestFixture]
    public abstract class IntegrationTestBase
    {
        protected WebApplicationFactory<Startup>? Factory;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

        [OneTimeSetUp]
        public void CreateWebHostFactory()
        {
            Factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(BuildWebHost);

            _httpClientFactoryMock
                .Setup(x => x.CreateClient(nameof(TestClient)))
                .Returns(Factory.CreateClient);
        }

        private void BuildWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(p => p.ClearProviders().AddDebug().AddConsole());
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("appsettings.json");
            });
            builder.ConfigureServices(services => services.AddTestWebApiClient());

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_httpClientFactoryMock.Object);
                ConfigureTestServices(services);
            });
        }

        protected virtual void ConfigureTestServices(IServiceCollection services)
        {
        }

        [OneTimeTearDown]
        public void DestroyWebHostFactory()
        {
            Factory?.Dispose();
        }

        protected async Task EnsureDatabaseCreated()
        {
            var contextFactory = Factory!.Services.GetRequiredService<Func<TestDbContext>>();
            await using var context = contextFactory();
            await contextFactory().Database.EnsureCreatedAsync();
        }

        protected async Task EnsureDatabaseDeleted()
        {
            var contextFactory = Factory!.Services.GetRequiredService<Func<TestDbContext>>();
            await using var context = contextFactory();
            await context.Database.EnsureDeletedAsync();
        }
    }
}
