using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Motix.Infrastructure.Persistence;

namespace Motix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDBContext(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<MotixDbContext>(options =>
            options.UseOracle(
                config.GetConnectionString("Default"),
                o => o.MigrationsAssembly("Motix.Infrastructure")));
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // repos futuros (não necessário agora)
        return services;
    }
}