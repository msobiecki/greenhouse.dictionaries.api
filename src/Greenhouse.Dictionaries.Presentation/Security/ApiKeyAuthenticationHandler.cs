using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Greenhouse.Dictionaries.Presentation.Security;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    public const string PolicyName = "CatalogWrite";
    public const string HeaderName = "X-API-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var providedValues) ||
            providedValues.Count != 1 ||
            string.IsNullOrWhiteSpace(providedValues[0]))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var configuredKey = configuration["Security:ApiKey"]!;
        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(providedValues[0]!));
        var configuredHash = SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
        if (!CryptographicOperations.FixedTimeEquals(providedHash, configuredHash))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "api-key-client")],
            SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
