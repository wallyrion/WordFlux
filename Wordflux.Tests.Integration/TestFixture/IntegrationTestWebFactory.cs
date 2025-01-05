using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WordFlux.ApiService;
using WordFlux.Application.Jobs;
using Wordflux.Tests.Integration.Extensions;

namespace Wordflux.Tests.Integration.TestFixture;

public class IntegrationTestWebFactory(DockerFixtures fixtures) : WebApplicationFactory<IApiMarker>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices((context, collection) =>
        {
            collection.RemoveAllImplementedBy<CardDetectLanguageBackgroundJob>();
            collection.RemoveAllImplementedBy<TestDistributedTracesBackgroundJob>();
            collection.RemoveAllImplementedBy<CardCreateTasksBackgroundJob>();
        });
        
        builder.ConfigureHostConfiguration(x =>
        {
            var collection = new[]
            {
                KeyValuePair.Create("ConnectionStrings:postgresdb", fixtures.Postgres.ConnectionString),
                KeyValuePair.Create("ConnectionStrings:KeysPersistenceBlobStorage", $"DefaultEndpointsProtocol=https;AccountName={AzuriteFixture.AccountName};AccountKey={AzuriteFixture.SharedKey};BlobEndpoint={fixtures.Azurite.ConnectionString};"),
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