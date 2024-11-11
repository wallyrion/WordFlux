using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WordFlux.Infrastructure.Persistence;

internal sealed class MigrationHostedService(IServiceProvider serviceProvider, ILogger<MigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        //await context.Database.EnsureCreatedAsync(cancellationToken);
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken: cancellationToken)).ToList();

        if (pendingMigrations.Count != 0)
        {
            logger.LogInformation("Migrating database.... {@PendingMigrations} pending migrations", pendingMigrations);
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migrated");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
