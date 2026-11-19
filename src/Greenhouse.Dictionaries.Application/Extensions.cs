using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MediatR;
using Greenhouse.Dictionaries.Application.Configuration.Validations;

namespace Greenhouse.Dictionaries.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(executingAssembly));
        services.AddAutoMapper(configuration => { }, executingAssembly);
        services.AddValidatorsFromAssembly(executingAssembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandValidationBehavior<,>));

        return services;
    }
}
