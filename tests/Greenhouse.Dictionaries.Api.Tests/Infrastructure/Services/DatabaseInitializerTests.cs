using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Infrastructure.Services;

public sealed class DatabaseInitializerTests
{
    [Fact]
    public async Task StartingInitializerMoreThanOncePreservesExistingData()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"greenhouse-initializer-{Guid.NewGuid():N}.db");
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:Provider"] = "Sqlite",
                    ["ConnectionStrings:Greenhouse"] = $"Data Source={databasePath}"
                })
                .Build();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddInfrastructure(configuration);
            await using var provider = services.BuildServiceProvider();
            var initializer = Assert.Single(provider.GetServices<IHostedService>());

            await initializer.StartAsync(CancellationToken.None);
            await using (var scope = provider.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                context.Plants.Add(new Plant
                {
                    ScientificName = "Rosa rugosa",
                    CommonName = "Rugosa rose",
                    Family = "Rosaceae",
                    Category = "flower",
                    Description = "A hardy flowering shrub.",
                    Sunlight = "Full sun",
                    Watering = "Regular"
                });
                await context.SaveChangesAsync();
            }

            await initializer.StartAsync(CancellationToken.None);

            await using var verificationScope = provider.CreateAsyncScope();
            var verificationContext = verificationScope.ServiceProvider.GetRequiredService<DatabaseContext>();
            Assert.Equal("Rugosa rose", await verificationContext.Plants.Select(plant => plant.CommonName).SingleAsync());
        }
        finally
        {
            foreach (var path in new[] { databasePath, $"{databasePath}-shm", $"{databasePath}-wal" })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
