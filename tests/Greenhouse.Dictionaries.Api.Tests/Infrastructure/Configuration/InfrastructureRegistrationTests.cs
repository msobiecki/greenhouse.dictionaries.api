using Greenhouse.Dictionaries.Infrastructure;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Infrastructure.Configuration;

public sealed class InfrastructureRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RequiresConnectionString()
    {
        var configuration = CreateConfiguration(null, null);
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddInfrastructure(configuration));

        Assert.Contains("ConnectionStrings:Greenhouse", exception.Message);
    }

    [Fact]
    public void AddInfrastructure_RejectsUnsupportedProvider()
    {
        var configuration = CreateConfiguration("Unsupported", "Data Source=unused.db");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => provider.GetRequiredService<DatabaseContext>());

        Assert.Contains("Unsupported database provider", exception.Message);
    }

    [Fact]
    public void AddInfrastructureConfiguresSqlServerWithoutOpeningConnection()
    {
        var configuration = CreateConfiguration(
            "SqlServer",
            "Server=localhost;Database=Greenhouse;User Id=sa;Password=NotARealPassword123!;TrustServerCertificate=True");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();

        var context = provider.GetRequiredService<DatabaseContext>();

        Assert.True(context.Database.IsSqlServer());
    }

    private static IConfiguration CreateConfiguration(string? provider, string? connectionString)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Database:Provider"] = provider,
            ["ConnectionStrings:Greenhouse"] = connectionString
        };
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }
}
