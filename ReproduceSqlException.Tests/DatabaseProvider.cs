using MartinCostello.SqlLocalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ReproduceSqlException.Tests
{
    public static class DatabaseProvider
    {
        internal static void ReplaceSqlServerWithReplacement<TContext>(IServiceCollection services, string dbName, bool useLocalDb)
            where TContext : DbContext
        {
            var dbContextOptions = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
            if (dbContextOptions is not null)
            {
                services.Remove(dbContextOptions);
            }

            var dbContext = services.SingleOrDefault(d => d.ServiceType == typeof(TContext));
            if (dbContext is not null)
            {
                services.Remove(dbContext);
            }

            var dbContextFactory = services.SingleOrDefault(d => d.ServiceType == typeof(Func<TContext>));
            if (dbContextFactory is not null)
            {
                services.Remove(dbContextFactory);
            }

            AddReplacement<TContext>(services, dbName, useLocalDb);
        }

        private static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });


        private static void AddReplacement<TContext>(IServiceCollection services, string dbName, bool useLocalDb)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(b => OptionsAction(services, b, dbName, useLocalDb), ServiceLifetime.Scoped, ServiceLifetime.Singleton);

            Func<IServiceProvider, Func<TContext>> implementationFactory = ImplementationFactory<TContext>;
            services.AddSingleton(implementationFactory);
        }

        static Func<TContext> ImplementationFactory<TContext>(IServiceProvider sp) where TContext : DbContext =>
            () => Activator.CreateInstance(typeof(TContext), sp.GetService<DbContextOptions<TContext>>()) as TContext
                  ?? throw new InvalidOperationException($"Could not call constructor of type {typeof(TContext)}");

        private static void OptionsAction(IServiceCollection serviceCollection, DbContextOptionsBuilder optionsBuilder, string dbName, bool useLocalDb)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();

            if (useLocalDb)
            {
                var sqlLocalDbApi = serviceProvider.GetRequiredService<ISqlLocalDbApi>();
                CreateLocalDb(optionsBuilder, dbName, sqlLocalDbApi);
            }
            else
            {
                CreateInMemoryDb(optionsBuilder, dbName);
            }
        }

        private static void CreateInMemoryDb(DbContextOptionsBuilder optionsBuilder, string dbName)
        {
            var root = new InMemoryDatabaseRoot();

            optionsBuilder
                .UseInMemoryDatabase(dbName, root)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(x =>
                {
                    x.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                });
        }

        private static void CreateLocalDb(DbContextOptionsBuilder optionsBuilder, string dbName, ISqlLocalDbApi localDb)
        {
            var instance = localDb.GetOrCreateInstance(dbName);
            var localDbInstanceManager = instance.Manage();

            if (!instance.IsRunning)
            {
                localDbInstanceManager.Start();
            }

            var connectionString = instance.GetConnectionString();

            optionsBuilder
                .UseSqlServer(connectionString)
                .UseLoggerFactory(MyLoggerFactory)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
        }

        public static async Task EnsureClearedLocalDb(Func<DbContext> contextFactory)
        {
            await using var context = contextFactory();
            var entityTypes = context.Model.GetEntityTypes();

            var method = typeof(DatabaseProvider)
                .GetTypeInfo()
                .GetMethods()
                .Single(m => m.Name == nameof(RemoveAllFromSet) && m.IsGenericMethod);

            foreach (var entityType in entityTypes)
            {
                method.MakeGenericMethod(entityType.ClrType).Invoke(null, new object[] { contextFactory });
            }
        }

        public static async Task RemoveAllFromSet<T>(Func<DbContext> contextFactory) where T : class
        {
            await using var context = contextFactory();
            var dbSet = context.Set<T>();
            dbSet.RemoveRange(dbSet);
            await context.SaveChangesAsync();
        }
    }
}
