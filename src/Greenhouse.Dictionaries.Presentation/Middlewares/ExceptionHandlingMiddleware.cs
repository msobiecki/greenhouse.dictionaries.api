using Greenhouse.Dictionaries.Presentation.Localization;
using FluentValidation;
using Greenhouse.Dictionaries.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Greenhouse.Dictionaries.Presentation.Middlewares;

public sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            logger.LogError(exception, "Request failed with an unhandled exception.");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var message = exception switch
        {
            PlantNotFoundException notFound => ApiMessages.PlantNotFound(notFound.PlantUid),
            PlantAlreadyExistsException alreadyExists => ApiMessages.PlantAlreadyExists(alreadyExists.PlantId),
            ValidationException => ApiMessages.InvalidCommand,
            BaseException => exception.Message,
            _ => ApiMessages.UnexpectedError
        };

        var response = new
        {
            message,
            status = statusCode,
            errors = GetErrors(exception)
        };
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            PlantNotFoundException => StatusCodes.Status404NotFound,
            PlantAlreadyExistsException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

    private static IReadOnlyDictionary<string, string[]> GetErrors(Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            return validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(ApiMessages.ValidationError).ToArray());
        }

        return new Dictionary<string, string[]>();
    }
}
