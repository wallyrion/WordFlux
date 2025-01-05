using Testcontainers.Azurite;

namespace Wordflux.Tests.Integration.Containers;

public class AzuriteFixture : IAsyncLifetime
{
    public const string AccountName = "devstoreaccount1";
    public const string SharedKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
    private const string BlobPort = "10000";
    private ushort _blobPublicPort;
    private string Host => _azuriteContainer.Hostname;
    public string ConnectionString => $"http://{Host}:{_blobPublicPort}/{AccountName}";
    
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
        _blobPublicPort = _azuriteContainer.GetMappedPublicPort(BlobPort);
    }

    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }
}