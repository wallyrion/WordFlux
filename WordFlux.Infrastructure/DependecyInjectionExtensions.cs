using Microsoft.Extensions.DependencyInjection;
using WordFlux.Infrastructure.Persistence;

namespace WordFlux.Infrastructure;

public static class DependecyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHostedService<MigrationHostedService>();

        return services;
    }
}