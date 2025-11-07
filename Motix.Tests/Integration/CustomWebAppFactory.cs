using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Motix.Infrastructure.Persistence;

namespace Motix.Tests.Integration;

public class CustomWebAppFactory : WebApplicationFactory<Motix.Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<MotixDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<MotixDbContext>(o => o.UseInMemoryDatabase("motix-tests"));

            services.AddSingleton<IConfiguration>(_ =>
            {
                var cfg = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ApiKey"] = "motix-secret-key"
                    })
                    .Build();
                return cfg;
            });
        });

        return base.CreateHost(builder);
    }

    /// <summary>Limpa e recria o banco InMemory entre testes.</summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MotixDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}