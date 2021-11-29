using MartinCostello.SqlLocalDb;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ReproduceSqlException.Db;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ReproduceSqlException.Tests
{
    public abstract class IntegrationTestBaseLocalDb : IntegrationTestBase
    {
        private const string ApplicationName = "ReproduceSqlException";
        private static readonly string InstanceName = ApplicationName + Guid.NewGuid();

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            services.AddSqlLocalDB(options => options.AutomaticallyDeleteInstanceFiles = true);
            DatabaseProvider.ReplaceSqlServerWithReplacement<TestDbContext>(services, InstanceName, useLocalDb: true);
        }

        [SetUp]
        public async Task Setup()
        {
            await EnsureDatabaseCreated();
            await EnsureClearedLocalDb();
        }

        private async Task EnsureClearedLocalDb()
        {
            var contextFactory = Factory!.Services.GetRequiredService<Func<TestDbContext>>();
            await DatabaseProvider.EnsureClearedLocalDb(contextFactory);
            Console.WriteLine($"Cleared database {InstanceName}");
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var sqlLocalDbApi = Factory?.Services?.GetService<ISqlLocalDbApi>();
            if (sqlLocalDbApi is null)
            {
                return;
            }

            var oldLocalDbInstances = sqlLocalDbApi
                .GetInstances()
                .Where(x => !x.IsRunning &&
                            !x.IsAutomatic &&
                            x.Name.StartsWith(ApplicationName) &&
                            x.LastStartTimeUtc < DateTime.UtcNow.AddHours(-1));

            foreach (var dbInstance in oldLocalDbInstances)
            {
                try
                {
                    sqlLocalDbApi.DeleteInstance(dbInstance.Name);
                    Console.WriteLine($"Deleted old instance {dbInstance.Name}");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
