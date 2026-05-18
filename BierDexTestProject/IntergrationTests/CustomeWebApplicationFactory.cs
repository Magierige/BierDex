using BierDex.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace BierDex.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(BierdexDBContext));
            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            var optionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BierdexDBContext>));
            if (optionsDescriptor != null) services.Remove(optionsDescriptor);

            var internalServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<BierdexDBContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
                options.UseInternalServiceProvider(internalServiceProvider); // Dwing het gebruik van de schone provider af
            });
        });
    }
}