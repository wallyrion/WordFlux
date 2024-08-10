using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService;

namespace TwitPoster.Web.WebHostServices;

internal sealed class MigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationHostedService> _logger;

    public MigrationHostedService(IServiceProvider serviceProvider, ILogger<MigrationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        //await context.Database.EnsureCreatedAsync(cancellationToken);
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken: cancellationToken)).ToList();

        if (pendingMigrations.Count != 0)
        {
            _logger.LogInformation("Migrating database.... {@PendingMigrations} pending migrations", pendingMigrations);
            await context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Database migrated");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
