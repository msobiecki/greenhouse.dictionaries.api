using System.Globalization;
using System.Text.Json;
using Greenhouse.Dictionaries.Presentation.Middlewares;
using Greenhouse.Dictionaries.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Presentation.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Theory]
    [InlineData("en-US", "Plant with ID '{0}' was not found.")]
    [InlineData("pl-PL", "Nie znaleziono ro\u015bliny o identyfikatorze '{0}'.")]
    public async Task PlantNotFoundExceptionReturnsLocalized404WithUid(string culture, string messageTemplate)
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            var uid = Guid.NewGuid();
            var context = new DefaultHttpContext();
            await using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            var middleware = new ExceptionHandlingMiddleware(NullLogger<ExceptionHandlingMiddleware>.Instance);

            await middleware.InvokeAsync(context, _ => Task.FromException(new PlantNotFoundException(uid)));

            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
            responseBody.Position = 0;
            using var response = await JsonDocument.ParseAsync(responseBody);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, messageTemplate, uid),
                response.RootElement.GetProperty("message").GetString());
            Assert.Equal(StatusCodes.Status404NotFound, response.RootElement.GetProperty("status").GetInt32());
            Assert.Empty(response.RootElement.GetProperty("errors").EnumerateObject());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public async Task UnknownExceptionReturnsGeneric500WithoutLeakingDetails()
    {
        var context = new DefaultHttpContext();
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        var middleware = new ExceptionHandlingMiddleware(NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context, _ =>
            Task.FromException(new InvalidOperationException("private implementation detail")));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody);
        var response = await reader.ReadToEndAsync();
        Assert.Contains("An unexpected error occurred.", response);
        Assert.DoesNotContain("private implementation detail", response);
    }

    [Fact]
    public async Task PlantAlreadyExistsExceptionReturns409()
    {
        var context = new DefaultHttpContext();
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        var middleware = new ExceptionHandlingMiddleware(NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context, _ =>
            Task.FromException(new PlantAlreadyExistsException(11)));

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody);
        Assert.Contains("11", await reader.ReadToEndAsync());
    }
}
