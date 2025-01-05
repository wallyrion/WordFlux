namespace Wordflux.Tests.Integration.Containers;

[Collection(nameof(SharedTestCollection))]
public class DockerFixtures : IAsyncLifetime
{
    public PostgresContainerFixture Postgres { get; } = new();
    public AzuriteFixture Azurite { get; } = new();
    
    public async Task InitializeAsync()
    {
        await Task.WhenAll(Postgres.InitializeAsync(), Azurite.InitializeAsync());
    }
    
    public async Task DisposeAsync()
    {
        await Task.WhenAll(Postgres.DisposeAsync(), Azurite.DisposeAsync());
    }
}