using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using WordFlux.ApiService;
using Wordflux.Tests.Integration.Containers;

namespace Wordflux.Tests.Integration.TestFixture;

public class IntegrationTestWebFactory(DockerFixtures fixtures) : WebApplicationFactory<IApiMarker>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(x =>
        {
            var collection = new[]
            {
                KeyValuePair.Create("ConnectionStrings:postgresdb", fixtures.Postgres.ConnectionString),
                KeyValuePair.Create("OpenAIKey", "open-ai-key"),
                KeyValuePair.Create("AzureAiTranslatorKey", "azure-ai-key"),
                KeyValuePair.Create("DeeplAuthKey", "deepl-auth-key"),
                KeyValuePair.Create("UnsplashApiKey", "unsplash-key"),
                KeyValuePair.Create("BingSearchApiKey", "bing-key"),
            };
            x.AddInMemoryCollection(collection!);
        });
        
        return base.CreateHost(builder);
    }
}