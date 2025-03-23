using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using Testcontainers.OpenSearch;
using WordFlux.Application.Common.Abstractions;
using WordFlux.Contracts;
using WordFlux.Domain.Domain;
using WordFlux.Infrastructure;
using WordFlux.Infrastructure.OpenSearch;

namespace Wordflux.Tests.Integration.OpensearchTests;

public class OpenSearchBasicTests 
{

    [Fact]
    public async Task OpenSearchBasic()
    {
        var opensearchContainer = new OpenSearchFixture();
        await opensearchContainer.InitializeAsync();

        var serviceCollection = new ServiceCollection()
            .AddLogging();

        var collection = new[]
        {
            KeyValuePair.Create("Opensearch:Url", opensearchContainer.GetConnectionString()),
            KeyValuePair.Create("Opensearch:SkipSslVerification", "true"),
            KeyValuePair.Create("Opensearch:Username", "admin"),
            KeyValuePair.Create("Opensearch:Password", "VeryStrongP@ssw0rd!")
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(collection!).Build();
        

        serviceCollection.AddOpenSearch(configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var scope = serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ISearchService>();
        await client.CreateIndexAsync();

        var card = CreateTestCard("Embankment", "берег");
        var card2 = CreateTestCard("по берегу", "on the coast", sourceLanguage: "ru", targetLanguage: "en");
        
        await client.AddAsync(card, CancellationToken.None);
        await client.AddAsync(card2, CancellationToken.None);
        
        var searchResult = await client.SearchAsync("берег", CancellationToken.None);
        searchResult.Should().HaveCount(2);
        searchResult.Should().ContainSingle(x => x.Term == "Embankment");
        searchResult.Should().ContainSingle(x => x.Term == "по берегу");
    }


    private static Card CreateTestCard(string term, string translation, string sourceLanguage = "en", string targetLanguage = "ru")
    {
        return new Card()
        {
            Id = Guid.NewGuid(),
            Status = CardProcessingStatus.Unprocessed,
            CreatedAt = DateTime.UtcNow,
            SourceLanguage = sourceLanguage,
            TargetLanguage = targetLanguage,
            Term = term,
            Translations = new List<CardTranslationItem>
            {
                new(translation)
            }
        };
    }
}
