using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Domain.Entities;
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

    [Theory]
    [InlineData("pl-PL", "pl-PL")]
    [InlineData("en-US", "en-US")]
    [InlineData("fr-FR", "en-US")]
    public async Task AcceptLanguageSelectsSupportedCultureAndContentLanguage(
        string acceptLanguage,
        string expectedContentLanguage)
    {
        using var factory = new PlantsApiFactory();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/plants?family=NoMatchingFamily");
        request.Headers.AcceptLanguage.ParseAdd(acceptLanguage);

        using var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(expectedContentLanguage, Assert.Single(response.Content.Headers.ContentLanguage));
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
        AssertAcceptLanguageParameter(plantsPath.GetProperty("get"));
        AssertAcceptLanguageParameter(plantsPath.GetProperty("post"));
        Assert.Equal(
            "X-API-Key",
            document.RootElement.GetProperty("components")
                .GetProperty("securitySchemes").GetProperty("ApiKey").GetProperty("name").GetString());
        Assert.True(plantsPath.GetProperty("post").GetProperty("responses").TryGetProperty("422", out _));
        Assert.True(plantByIdPath.GetProperty("put").GetProperty("responses").TryGetProperty("404", out _));
        Assert.True(plantByIdPath.GetProperty("put").GetProperty("responses").TryGetProperty("422", out _));
        Assert.True(plantByIdPath.GetProperty("delete").GetProperty("responses").TryGetProperty("404", out _));
    }

    private static void AssertAcceptLanguageParameter(JsonElement operation)
    {
        var parameter = Assert.Single(operation.GetProperty("parameters").EnumerateArray(), item =>
            item.GetProperty("name").GetString() == "Accept-Language");
        Assert.Equal("header", parameter.GetProperty("in").GetString());
        Assert.False(parameter.TryGetProperty("required", out var required) && required.GetBoolean());

        var schema = parameter.GetProperty("schema");
        Assert.Equal("en-US", schema.GetProperty("default").GetString());
        Assert.Equal(
            new[] { "en-US", "pl-PL" },
            schema.GetProperty("enum").EnumerateArray().Select(value => value.GetString()).ToArray());
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
        Assert.Equal("East Asia", createdPlant.NativeRange);
        Assert.Equal("Coastal forests", createdPlant.Habitat);
        Assert.Equal(15m, createdPlant.TemperatureMinC);
        Assert.Equal(27m, createdPlant.TemperatureMaxC);
        Assert.Equal(50m, createdPlant.HumidityMinPercent);
        Assert.Equal(70m, createdPlant.HumidityMaxPercent);
        Assert.Equal("Avoid cold drafts.", createdPlant.EnvironmentalNotes);
        Assert.Equal(SunlightRequirement.BrightIndirectLight, createdPlant.SunlightRequirement);
        Assert.Equal(4m, createdPlant.SunlightHoursMin);
        Assert.Equal(6m, createdPlant.SunlightHoursMax);
        Assert.Equal(WateringRequirement.WaterWhenTopLayerDry, createdPlant.WateringRequirement);
        Assert.Equal("Water thoroughly and let excess drain away.", createdPlant.WateringInstructions);
        Assert.Equal("Reduce watering during winter dormancy.", createdPlant.SeasonalWateringNotes);
        Assert.Equal("Feed monthly during active growth.", createdPlant.FertilizingInstructions);
        Assert.Equal("A dense shrub with arching thorny stems.", createdPlant.DetailedDescription);
        Assert.Equal("Shrub", createdPlant.GrowthHabit);
        Assert.Equal(PlantLifeCycle.Perennial, createdPlant.LifeCycle);
        Assert.Equal("Beach rose", Assert.Single(createdPlant.AlternativeNames));
        Assert.Equal(180m, createdPlant.MatureHeightCm);
        Assert.Equal("CC BY 4.0", Assert.Single(createdPlant.Images).License);
        var rabbitAssessment = Assert.Single(
            createdPlant.ToxicityAssessments, assessment => assessment.TargetCode == "rabbit");
        Assert.Equal(ToxicityLevel.Caution, rabbitAssessment.Level);
        var catAssessment = Assert.Single(
            createdPlant.ToxicityAssessments, assessment => assessment.TargetCode == "cat");
        Assert.Equal("Example plant registry", Assert.Single(catAssessment.Sources).Publisher);

        var discoverResponse = await client.GetAsync(
            "/api/plants?search=thorny&category=Flowering&family=Rosaceae" +
            "&sunlightRequirement=BrightIndirectLight&wateringRequirement=WaterWhenTopLayerDry" +
            "&toxicityTarget=cat&toxicityLevel=Toxic&temperatureC=20&humidityPercent=60");
        Assert.Equal(HttpStatusCode.OK, discoverResponse.StatusCode);
        var discoveredPlants = await discoverResponse.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();
        Assert.Equal(createdPlant.Id, Assert.Single(discoveredPlants!.Items).Id);
        Assert.Equal(1, discoveredPlants.TotalCount);
        Assert.False(discoveredPlants.HasNextPage);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/plants/{createdPlant.Id}", CreatePlant("Updated rose", "Japanese rose"));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/plants/{createdPlant.Id}");
        var updatedPlant = await getResponse.Content.ReadFromJsonAsync<PlantDto>();
        Assert.Equal("Updated rose", updatedPlant?.CommonName);
        Assert.Equal("East Asia", updatedPlant?.NativeRange);
        Assert.Equal("Coastal forests", updatedPlant?.Habitat);
        Assert.Equal(15m, updatedPlant?.TemperatureMinC);
        Assert.Equal(70m, updatedPlant?.HumidityMaxPercent);
        Assert.Equal(WateringRequirement.WaterWhenTopLayerDry, updatedPlant?.WateringRequirement);
        Assert.Equal("Repot every two to three years in spring.", updatedPlant?.RepottingInstructions);
        Assert.Equal("Fragrant pink flowers in early summer.", updatedPlant?.FlowerDescription);
        Assert.Equal("Japanese rose", Assert.Single(updatedPlant!.AlternativeNames));
        Assert.Contains(updatedPlant.ToxicityAssessments, assessment => assessment.TargetCode == "rabbit");

        var deleteResponse = await client.DeleteAsync($"/api/plants/{createdPlant.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/plants/{createdPlant.Id}")).StatusCode);
    }

    [Fact]
    public async Task PlantTranslationsAreSelectedByAcceptLanguageAndReportContentLanguage()
    {
        using var factory = new PlantsApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);

        var createResponse = await client.PostAsJsonAsync("/api/plants", CreatePlant());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal("en-US", Assert.Single(createResponse.Content.Headers.ContentLanguage));
        var englishPlant = await createResponse.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(englishPlant);

        using var fallbackRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/plants/{englishPlant.Id}");
        fallbackRequest.Headers.AcceptLanguage.ParseAdd("pl-PL");
        using var fallbackResponse = await client.SendAsync(fallbackRequest);
        var fallbackPlant = await fallbackResponse.Content.ReadFromJsonAsync<PlantDto>();
        Assert.Equal("en-US", Assert.Single(fallbackResponse.Content.Headers.ContentLanguage));
        Assert.Equal("Rugosa rose", fallbackPlant?.CommonName);

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/plants/{englishPlant.Id}")
        {
            Content = JsonContent.Create(CreatePlant(
                "Róża pomarszczona", "Róża japońska", "Szczegóły toksyczności."))
        };
        updateRequest.Headers.AcceptLanguage.ParseAdd("pl-PL");
        using var updateResponse = await client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        using var polishRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/plants/{englishPlant.Id}");
        polishRequest.Headers.AcceptLanguage.ParseAdd("pl-PL");
        using var polishResponse = await client.SendAsync(polishRequest);
        var polishPlant = await polishResponse.Content.ReadFromJsonAsync<PlantDto>();
        Assert.Equal("pl-PL", Assert.Single(polishResponse.Content.Headers.ContentLanguage));
        Assert.Equal("Róża pomarszczona", polishPlant?.CommonName);
        Assert.Equal("Róża japońska", Assert.Single(polishPlant!.AlternativeNames));
        Assert.Equal("Szczegóły toksyczności.", Assert.Single(polishPlant.ToxicityAssessments,
            assessment => assessment.TargetCode == "cat").Details);

        using var exactLanguageRequest = new HttpRequestMessage(
            HttpMethod.Get, "/api/plants?onlyRequestedLanguage=true");
        exactLanguageRequest.Headers.AcceptLanguage.ParseAdd("pl-PL");
        using var exactLanguageResponse = await client.SendAsync(exactLanguageRequest);
        var exactLanguagePage = await exactLanguageResponse.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();
        Assert.Equal(1, exactLanguagePage?.TotalCount);
        Assert.Equal(englishPlant.Id, Assert.Single(exactLanguagePage!.Items).Id);

        using var englishRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/plants/{englishPlant.Id}");
        englishRequest.Headers.AcceptLanguage.ParseAdd("en-US");
        using var englishResponse = await client.SendAsync(englishRequest);
        var unchangedEnglishPlant = await englishResponse.Content.ReadFromJsonAsync<PlantDto>();
        Assert.Equal("en-US", Assert.Single(englishResponse.Content.Headers.ContentLanguage));
        Assert.Equal("Rugosa rose", unchangedEnglishPlant?.CommonName);
        Assert.Equal("Beach rose", Assert.Single(unchangedEnglishPlant!.AlternativeNames));
        Assert.Equal("Test toxicity details.", Assert.Single(unchangedEnglishPlant.ToxicityAssessments,
            assessment => assessment.TargetCode == "cat").Details);
    }

    [Fact]
    public async Task GetPlantsSupportsOffsetAndLimit()
    {
        using var factory = new PlantsApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);

        var firstResponse = await client.PostAsJsonAsync("/api/plants", CreatePlant("Paging plant one"));
        var secondResponse = await client.PostAsJsonAsync("/api/plants", CreatePlant("Paging plant two"));
        var thirdResponse = await client.PostAsJsonAsync("/api/plants", CreatePlant("Paging plant three"));
        var second = await secondResponse.Content.ReadFromJsonAsync<PlantDto>();

        using var pageResponse = await client.GetAsync("/api/plants?offset=1&limit=1");
        var page = await pageResponse.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, thirdResponse.StatusCode);
        Assert.Equal(second!.Id, Assert.Single(page!.Items).Id);
        Assert.Equal(3, page.TotalCount);
        Assert.True(page.HasPreviousPage);
        Assert.True(page.HasNextPage);
        Assert.Equal(2, page.NextOffset);

        var lastPage = await client.GetFromJsonAsync<PagedResult<PlantDto>>("/api/plants?offset=2&limit=1");
        Assert.NotNull(lastPage);
        Assert.False(lastPage.HasNextPage);
        Assert.Null(lastPage.NextOffset);

        using var fallbackRequest = new HttpRequestMessage(HttpMethod.Get, "/api/plants?offset=0&limit=10");
        fallbackRequest.Headers.AcceptLanguage.ParseAdd("pl-PL");
        using var fallbackResponse = await client.SendAsync(fallbackRequest);
        var fallbackPage = await fallbackResponse.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();
        Assert.Equal(3, fallbackPage?.TotalCount);
        Assert.All(fallbackPage!.Items, plant => Assert.Equal("en-US", plant.Language));

        using var exactRequest = new HttpRequestMessage(
            HttpMethod.Get, "/api/plants?offset=0&limit=10&onlyRequestedLanguage=true");
        exactRequest.Headers.AcceptLanguage.ParseAdd("pl-PL");
        using var exactResponse = await client.SendAsync(exactRequest);
        var exactPage = await exactResponse.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();
        Assert.Empty(exactPage!.Items);
        Assert.Equal(0, exactPage.TotalCount);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/plants?offset=-1")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/plants?limit=101")).StatusCode);
    }

    [Fact]
    public async Task ValidationErrorsUseNegotiatedLanguage()
    {
        using var factory = new PlantsApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/plants")
        {
            Content = JsonContent.Create(CreatePlant(" "))
        };
        request.Headers.AcceptLanguage.ParseAdd("pl-PL");

        using var response = await client.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal((HttpStatusCode)422, response.StatusCode);
        Assert.Equal("pl-PL", Assert.Single(response.Content.Headers.ContentLanguage));
        Assert.Contains("Polecenie zawiera nieprawidłowe dane.", responseBody);
        Assert.Contains("jest wymagane.", responseBody);
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

        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/plants?temperatureC=100")).StatusCode);
    }

    private static PlantInputDto CreatePlant(
        string commonName = "Rugosa rose",
        string alternativeName = "Beach rose",
        string toxicityDetails = "Test toxicity details.") => new(
        "Rosa rugosa",
        commonName,
        [alternativeName],
        "Rosaceae",
        PlantType.Flowering,
        "A hardy flowering shrub.",
        "Thorny arching stems and fragrant pink flowers.",
        NativeRange: "East Asia",
        Habitat: "Coastal forests",
        TemperatureMinC: 15,
        TemperatureMaxC: 27,
        HumidityMinPercent: 50,
        HumidityMaxPercent: 70,
        EnvironmentalNotes: "Avoid cold drafts.",
        SunlightRequirement: SunlightRequirement.BrightIndirectLight,
        SunlightHoursMin: 4,
        SunlightHoursMax: 6,
        MatureHeightCm: 180,
        MatureWidthCm: 250,
        ToxicityAssessments:
        [
            new("cat", ToxicityLevel.Toxic, toxicityDetails,
                [new("https://example.test/toxicity", "Example plant registry")]),
            new("dog", ToxicityLevel.NoKnownRisk),
            new("human", ToxicityLevel.NoKnownRisk),
            new("rabbit", ToxicityLevel.Caution, "Test caution details.")
        ],
        Images: [new PlantImageInputDto(
            "https://example.test/rose.jpg",
            "Botanical archive",
            "CC BY 4.0",
            "Example author")],
        DetailedDescription: "A dense shrub with arching thorny stems.",
        GrowthHabit: "Shrub",
        FoliageDescription: "Glossy, wrinkled leaves with serrated edges.",
        FlowerDescription: "Fragrant pink flowers in early summer.",
        FloweringPeriod: "Late spring to early summer",
        FruitDescription: "Large red rose hips develop in autumn.",
        LifeCycle: PlantLifeCycle.Perennial,
        WateringRequirement: WateringRequirement.WaterWhenTopLayerDry,
        WateringInstructions: "Water thoroughly and let excess drain away.",
        SeasonalWateringNotes: "Reduce watering during winter dormancy.",
        FertilizingInstructions: "Feed monthly during active growth.",
        PruningInstructions: "Remove dead or damaged stems after flowering.",
        RepottingInstructions: "Repot every two to three years in spring.");

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
            builder.UseSetting("Security:ApiKey", ApiKey);
            builder.UseSetting("Database:Provider", "Sqlite");
            builder.UseSetting("ConnectionStrings:Greenhouse", $"Data Source={databasePath}");
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
