using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WordFlux.Application.Common.Behaviours;
using WordFlux.Application.Decks.Commands;

namespace WordFlux.Application.Common;

public static class DependecyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services
            .AddMediatR(x =>
            {
                x.RegisterServicesFromAssembly(assembly);
                x.AddOpenBehavior(typeof(ValidationPipelineBehaviour<,>));
            });

        services.AddValidatorsFromAssemblyContaining<CreateDeckCommand>();
        
        return services;
    }
}