using Greenhouse.Dictionaries.Presentation.Controllers;
using Greenhouse.Dictionaries.Presentation.Middlewares;
using Greenhouse.Dictionaries.Presentation.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Greenhouse.Dictionaries.Presentation;

public static class Extensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiKey = configuration["Security:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < 32)
        {
            throw new InvalidOperationException("Security:ApiKey must contain at least 32 characters.");
        }

        services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, _ => { });
        services.AddAuthorization(options =>
            options.AddPolicy(ApiKeyAuthenticationHandler.PolicyName, policy =>
                policy.AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName)
                    .RequireAuthenticatedUser()));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swagger =>
        {
            swagger.EnableAnnotations();
            swagger.AddSecurityDefinition(ApiKeyAuthenticationHandler.SchemeName, new OpenApiSecurityScheme
            {
                Description = "API key for catalog write operations.",
                Name = ApiKeyAuthenticationHandler.HeaderName,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            swagger.OperationFilter<ApiKeyOperationFilter>();
        });

        services.AddControllers();
        services.AddTransient<ExceptionHandlingMiddleware>();
        
        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.MapControllers();
        return app;
    }
}
