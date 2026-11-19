using System.Text.Json.Nodes;
using Greenhouse.Dictionaries.Application.Configuration;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Greenhouse.Dictionaries.Presentation.Configuration;

public sealed class AcceptLanguageOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];
        if (operation.Parameters.Any(parameter =>
                parameter.In == ParameterLocation.Header &&
                string.Equals(parameter.Name, "Accept-Language", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Description = "Preferred language for the response.",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Default = JsonValue.Create(LanguageConfiguration.DefaultCulture),
                Enum = LanguageConfiguration.CreateSupportedCultures()
                    .Select(culture => (JsonNode)JsonValue.Create(culture.Name)!)
                    .ToList()
            }
        });
    }
}
