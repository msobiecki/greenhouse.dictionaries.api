using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Presentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Api;

public sealed class PlantsApiIntegrationTests
{
    private const string ApiKey = "test-api-key-for-integration-tests-32chars";

    [Fact]
    public async Task ReadIsAnonymousAndAllWriteRoutesRequireApiKey()
    {
        using var factory = new PlantsApiFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/plants")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/plants", CreatePlant())).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PutAsJsonAsync("/api/plants/1", CreatePlant())).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.DeleteAsync("/api/plants/1")).StatusCode);

        client.DefaultRequestHeaders.Add("X-API-Key", "incorrect-api-key-for-tests-32chars");
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/plants", CreatePlant())).StatusCode);
    }

    [Fact]
    public async Task SwaggerDocumentsApiKeyForWriteOperationsOnly()
    {
        using var factory = new PlantsApiFactory();
        using var client = factory.CreateClient();

        using var document = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
        var paths = document.RootElement.GetProperty("paths");
        var plantsPath = paths.GetProperty("/api/plants");
        var plantByIdPath = paths.GetProperty("/api/plants/{id}");

        Assert.False(plantsPath.GetProperty("get").TryGetProperty("security", out _));
        Assert.True(plantsPath.GetProperty("post").TryGetProperty("security", out _));
        Assert.Equal(
            "X-API-Key",
            document.RootElement.GetProperty("components")
                .GetProperty("securitySchemes").GetProperty("ApiKey").GetProperty("name").GetString());
        Assert.True(plantsPath.GetProperty("post").GetProperty("responses").TryGetProperty("422", out _));
        Assert.True(plantByIdPath.GetProperty("put").GetProperty("responses").TryGetProperty("404", out _));
        Assert.True(plantByIdPath.GetProperty("put").GetProperty("responses").TryGetProperty("422", out _));
        Assert.True(plantByIdPath.GetProperty("delete").GetProperty("responses").TryGetProperty("404", out _));
    }

    [Fact]
    public async Task ProductionPipelineEnablesHttpsRedirection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:ApiKey"] = ApiKey,
                ["https_port"] = "8443"
            })
            .Build();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Production"
        });
        builder.Configuration.AddConfiguration(configuration);
        builder.WebHost.UseTestServer();
        builder.Services.AddPresentation(builder.Configuration);
        var app = builder.Build();
        app.UsePresentation();
        app.Run(context => Task.CompletedTask);
        await app.StartAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/not-mapped");

        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.StartsWith("https://", response.Headers.Location?.ToString());
        await app.StopAsync();
        await app.DisposeAsync();
    }

    [Fact]
    public async Task AuthorizedClientCanCreateUpdateAndDeletePlant()
    {
        using var factory = new PlantsApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);

        var createResponse = await client.PostAsJsonAsync("/api/plants", CreatePlant());

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdPlant = await createResponse.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(createdPlant);
        Assert.True(createdPlant.Id > 0);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/plants/{createdPlant.Id}", CreatePlant("Updated rose"));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/plants/{createdPlant.Id}");
        var updatedPlant = await getResponse.Content.ReadFromJsonAsync<PlantDto>();
        Assert.Equal("Updated rose", updatedPlant?.CommonName);

        var deleteResponse = await client.DeleteAsync($"/api/plants/{createdPlant.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/plants/{createdPlant.Id}")).StatusCode);
    }

    [Fact]
    public async Task ValidationAndNotFoundExceptionsHaveExpectedHttpResponses()
    {
        using var factory = new PlantsApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);

        var invalidResponse = await client.PostAsJsonAsync("/api/plants", CreatePlant(" "));
        Assert.Equal((HttpStatusCode)422, invalidResponse.StatusCode);
        Assert.Contains("CommonName", await invalidResponse.Content.ReadAsStringAsync());

        var missingResponse = await client.DeleteAsync("/api/plants/404");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
        Assert.Contains("404", await missingResponse.Content.ReadAsStringAsync());
    }

    private static PlantInputDto CreatePlant(string commonName = "Rugosa rose") => new(
        "Rosa rugosa",
        commonName,
        "Rosaceae",
        "flower",
        "A hardy flowering shrub.",
        "Full sun",
        "Water during dry periods.");

    private sealed class PlantsApiFactory : WebApplicationFactory<Program>
    {
        private readonly string environment;
        private readonly string databasePath = Path.Combine(
            Path.GetTempPath(), $"greenhouse-api-{Guid.NewGuid():N}.db");

        public PlantsApiFactory(string environment = "Development")
        {
            this.environment = environment;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:Provider"] = "Sqlite",
                    ["ConnectionStrings:Greenhouse"] = $"Data Source={databasePath}",
                    ["Security:ApiKey"] = ApiKey,
                    ["https_port"] = "8443"
                }));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
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
}
