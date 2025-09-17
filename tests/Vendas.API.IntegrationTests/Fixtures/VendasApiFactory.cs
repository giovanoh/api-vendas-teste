using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Vendas.API.Infrastructure.Contexts;

namespace Vendas.API.IntegrationTests.Fixtures;

public class VendasApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"vendas_api_test_db_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var options = services.Where(r => (r.ServiceType == typeof(DbContextOptions))
              || (r.ServiceType.IsGenericType && r.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))).ToArray();
            foreach (var option in options)
            {
                services.Remove(option);
            }

            var context = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ApiDbContext));
            if (context != null)
            {
                services.Remove(context);
            }

            services.AddDbContext<ApiDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            try
            {
                db.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred seeding the database.", ex);
            }
        });
    }
}