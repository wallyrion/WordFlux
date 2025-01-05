using System.Net.Http.Json;
using FluentAssertions;
using WordFlux.Contracts;
using WordFlux.Domain.Domain;

namespace Wordflux.Tests.Integration.Tests.Decks;

public class DeleteDeckTests(DockerFixtures fixtures) : BaseIntegrationTest(fixtures)
{
    [Fact]
    public async Task Should_Remove_Successfully()
    {
        await AddDefaultAuthAsync();

        var createDeckRequest = new CreateDeckRequest("my_deck");
        var createDeckResponse = await ApiClient.PostAsJsonAsync("decks", createDeckRequest);

        var deck = await createDeckResponse.Content.ReadFromJsonAsync<Deck>();

        var deleteDeckResponse = await ApiClient.DeleteAsync($"decks/{deck!.Id}");
        deleteDeckResponse.Should().Be204NoContent();
        
        var deletedDeck = await ApiClient.GetAsync($"/decks/{deck.Id}");
        deletedDeck.Should().Be404NotFound();
    }
    
    [Fact]
    public async Task Should_Be_Unauthorized()
    {
        var deckRequest = new PatchDeckRequest(null, true);
        var response = await ApiClient.PatchAsJsonAsync($"decks/{Guid.NewGuid()}", deckRequest);
        response.Should().Be401Unauthorized();
    }
    
    [Fact]
    public async Task Should_Return_BadRequest_When_Deck_Is_Created_By_Another_User()
    {
        using var anotherClient = await CreateUserClientAsync();
        var deckRequest = new CreateDeckRequest("my_deck");
        
        var createDeckResponse = await anotherClient.PostAsJsonAsync("decks", deckRequest);
        var createdDeck = await createDeckResponse.Content.ReadFromJsonAsync<Deck>();
        
        await AddDefaultAuthAsync();
        
        var deleteDeckResponse = await ApiClient.DeleteAsync($"decks/{createdDeck!.Id}");
        deleteDeckResponse.Should().Be400BadRequest();
        
        var myDeck = await anotherClient.GetFromJsonAsync<DeckDto>($"/decks/{createdDeck!.Id}");
        myDeck.Should().NotBeNull();
    }
}