using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Greenhouse.Dictionaries.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;

namespace Greenhouse.Dictionaries.Infrastructure;

public static class Extensions
{
    public static ConfigureHostBuilder UseInfrastructure(this ConfigureHostBuilder hostBuilder)
    {
        hostBuilder.UseNLog();
        return hostBuilder;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPlantWriteRepository, PlantWriteRepository>();
        services.AddScoped<IPlantReadRepository, PlantReadRepository>();

        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("Greenhouse");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Greenhouse must be configured.");
        }

        services.AddDbContext<DatabaseContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "sqlite":
                    options.UseSqlite(connectionString)
                        .ConfigureWarnings(warnings =>
                            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
                    break;
                case "sqlserver":
                    options.UseSqlServer(connectionString);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
            }
        });

        services.AddHostedService<DatabaseInitializer>();

        return services;
    }
}
