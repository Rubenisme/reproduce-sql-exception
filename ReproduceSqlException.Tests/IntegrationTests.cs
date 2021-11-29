using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ReproduceSqlException.Client;
using ReproduceSqlException.Db;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReproduceSqlException.Tests
{
    public class IntegrationTests : IntegrationTestBaseLocalDb
    {
        [Test]
        public async Task Test()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Inconclusive("This test can only be run on Windows.");
            }

            // Arrange
            var client = Factory!.Services.GetRequiredService<TestClient>();
            var contextFactory = Factory!.Services.GetRequiredService<Func<TestDbContext>>();

            await using var context = contextFactory();
            var connectionString = context.Database.GetConnectionString();
            await context.Database.OpenConnectionAsync();

            // Act
            var result = await client.TestMethod(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("Hello world"));
        }
    }
}
