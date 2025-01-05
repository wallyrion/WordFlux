namespace Wordflux.Tests.Integration.Containers;

[Collection(nameof(SharedTestCollection))]
public class DockerFixtures : IAsyncLifetime
{
    public PostgresContainerFixture Postgres { get; } = new();
    
    public async Task InitializeAsync()
    {
        await Postgres.InitializeAsync();
    }
    
    public async Task DisposeAsync()
    {
        await Postgres.DisposeAsync();
    }
}