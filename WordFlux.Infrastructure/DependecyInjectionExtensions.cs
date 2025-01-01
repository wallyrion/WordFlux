using Microsoft.Extensions.DependencyInjection;
using WordFlux.Application;
using WordFlux.Infrastructure.Authorization;
using WordFlux.Infrastructure.Persistence;

namespace WordFlux.Infrastructure;

public static class DependecyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHostedService<MigrationHostedService>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}