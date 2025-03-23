namespace Testcontainers.OpenSearch;

public class OpenSearchFixture : IAsyncLifetime, IAsyncDisposable
{
    private readonly OpenSearchContainer _opensearchContainer = new OpenSearchBuilder().Build();

    public string GetConnectionString()
    {
        return _opensearchContainer.GetConnection();
    }
    
    
    public Task InitializeAsync()
    {
        return _opensearchContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _opensearchContainer.DisposeAsync().AsTask();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DisposeAsync();
    }
}