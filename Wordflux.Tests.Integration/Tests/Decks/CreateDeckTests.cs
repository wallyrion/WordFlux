using System.Net.Http.Json;
using FluentAssertions;
using WordFlux.Contracts;
using WordFlux.Domain.Domain;
using Wordflux.Tests.Integration.Containers;
using Wordflux.Tests.Integration.TestFixture;

namespace Wordflux.Tests.Integration.Tests.Decks;

public class CreateDeckTests(DockerFixtures fixtures) : BaseIntegrationTest(fixtures)
{
    [Fact]
    public async Task Should_Create_Deck_Successfully()
    {
        await AddDefaultAuthAsync();

        var deckRequest = new CreateDeckRequest("my_deck");
        var createDeckResponse = await ApiClient.PostAsJsonAsync("decks", deckRequest);
        createDeckResponse.Should().Be200Ok();

        var deck = await createDeckResponse.Content.ReadFromJsonAsync<Deck>();
        deck!.Name.Should().Be(deckRequest.Name);
        deck.Id.Should().NotBeEmpty();

        var myDecks = await ApiClient.GetFromJsonAsync<IReadOnlyList<DeckDto>>("/decks");
        var createdDeck = myDecks.Should().ContainSingle().Subject;
        createdDeck.Name.Should().Be(deckRequest.Name);
    }
}