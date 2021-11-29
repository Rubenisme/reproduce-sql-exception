using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ReproduceSqlException.Db
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTestDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<TestDbContext>(builder =>
            {
                builder.UseSqlServer(connectionString, o => o.UseRelationalNulls());
#if DEBUG
                builder.EnableSensitiveDataLogging();
                builder.EnableDetailedErrors();
#endif
            }, ServiceLifetime.Scoped, ServiceLifetime.Singleton);

            services.AddSingleton<Func<TestDbContext>>(sp => () => new(sp.GetRequiredService<DbContextOptions<TestDbContext>>()));
        }
    }
}
