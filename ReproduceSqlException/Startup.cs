using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReproduceSqlException.Db;

namespace ReproduceSqlException
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddControllersAsServices();

            const string connectionString = "Server=tcp:database,1433;Database=TestDatabase;Integrated Security=True;App=ReproduceSqlException;MultipleActiveResultSets=True";
            services.AddTestDbContext(connectionString);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", context => context.Response.WriteAsync("Hello World"));
                endpoints.MapControllers();
            });
        }
    }
}
