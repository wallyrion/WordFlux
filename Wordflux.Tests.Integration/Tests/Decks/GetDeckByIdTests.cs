using System.Net.Http.Json;
using FluentAssertions;
using WordFlux.Contracts;
using WordFlux.Domain.Domain;
using Wordflux.Tests.Integration.Extensions;

namespace Wordflux.Tests.Integration.Tests.Decks;

public class GetDeckByIdTests(DockerFixtures fixtures) : BaseIntegrationTest(fixtures)
{
    [Fact]
    public async Task Should_Return_DeckDto_Successfully_When_Request_By_Same_User_That_Created_Deck()
    {
        await AddDefaultAuthAsync();

        var deckRequest = new CreateDeckRequest("my_deck");
        var createDeckResponse = await ApiClient.PostAsJsonAsync("decks", deckRequest);

        var createdDeck = await createDeckResponse.Content.ReadFromJsonAsync<Deck>();

        var myDeck = await ApiClient.GetFromJsonAsync<DeckDto>($"/decks/{createdDeck!.Id}");
        myDeck.Should().BeEquivalentTo(new
        {
            Id = createdDeck.Id,
            Name = "my_deck",
            CardsCount = 0,
            Type = DeckType.Custom,
            IsPublic = false,
            IsEditable = true
        });
    }
    
    [Fact]
    public async Task Should_Return_Not_Found_When_Deck_Is_Created_By_Another_User_And_Not_Public()
    {
        using var anotherClient = await CreateUserClientAsync();
        var deckRequest = new CreateDeckRequest("my_deck");
        
        var createDeckResponse = await anotherClient.PostAsJsonAsync("decks", deckRequest);
        var createdDeck = await createDeckResponse.Content.ReadFromJsonAsync<Deck>();
        
        await AddDefaultAuthAsync();

        var deck = await ApiClient.GetAsync($"/decks/{createdDeck!.Id}");
        deck.Should().Be404NotFound();
    }
    
    
    [Fact]
    public async Task Should_Return_Public_Deck_For_Another_User()
    {
        using var anotherClient = await CreateUserClientAsync();
        var deckRequest = new CreateDeckRequest("my_deck");
        
        var createdDeck = await anotherClient.PostAsJsonAsync("decks", deckRequest).WaitForJson<Deck>();
        var patchDeckRequest = new PatchDeckRequest(IsPublic: true);
        await anotherClient.PatchAsJsonAsync($"decks/{createdDeck!.Id}", patchDeckRequest).WaitForSuccess();
        
        await AddDefaultAuthAsync();

        var deck = await ApiClient.GetFromJsonAsync<DeckDto>($"/decks/{createdDeck!.Id}");
        deck.Should().BeEquivalentTo(new
        {
            deckRequest.Name,
            createdDeck.Id,
        });
    }
    
    [Fact]
    public async Task Should_Return_Public_Deck_For_Incognito()
    {
        using var anotherClient = await CreateUserClientAsync();
        var deckRequest = new CreateDeckRequest("my_deck");
        
        var createdDeck = await anotherClient.PostAsJsonAsync("decks", deckRequest).WaitForJson<Deck>();
        var patchDeckRequest = new PatchDeckRequest(IsPublic: true);
        await anotherClient.PatchAsJsonAsync($"decks/{createdDeck!.Id}", patchDeckRequest).WaitForSuccess();
        
        var deck = await ApiClient.GetFromJsonAsync<DeckDto>($"/decks/{createdDeck!.Id}");
        deck.Should().BeEquivalentTo(new
        {
            deckRequest.Name,
            createdDeck.Id,
        });
    }
}