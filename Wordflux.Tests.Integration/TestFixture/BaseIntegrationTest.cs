using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Wordflux.Tests.Integration.Containers;

namespace Wordflux.Tests.Integration.TestFixture;

[Collection(nameof(SharedTestCollection))]
public abstract class BaseIntegrationTest(DockerFixtures fixtures) : IAsyncLifetime
{
    private readonly IntegrationTestWebFactory _factory = new(fixtures);
    private AsyncServiceScope _scope;
    private Respawner _respawner = null!;
    private NpgsqlConnection _dbConnection = null!;

    protected HttpClient ApiClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        ApiClient = _factory.CreateDefaultClient();

        _scope = _factory.Services.CreateAsyncScope();
        
        _dbConnection = new NpgsqlConnection(fixtures.Postgres.ConnectionString);
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = [new ("__EFMigrationsHistory")]
        });
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        ApiClient.Dispose();

        await _respawner.ResetAsync(_dbConnection);
        await _dbConnection.DisposeAsync();
    }
}