using System.Net.Http.Json;
using FluentAssertions;
using WordFlux.Contracts;
using WordFlux.Domain.Domain;
using Wordflux.Tests.Integration.Extensions;

namespace Wordflux.Tests.Integration.Tests.Decks;

public class PatchDeckTests(DockerFixtures fixtures) : BaseIntegrationTest(fixtures)
{
    [Fact]
    public async Task Should_Rename_Successfully()
    {
        await AddDefaultAuthAsync();

        var createDeckRequest = new CreateDeckRequest("my_deck");
        var createDeckResponse = await ApiClient.PostAsJsonAsync("decks", createDeckRequest);

        var deck = await createDeckResponse.Content.ReadFromJsonAsync<Deck>();

        var patchDeckRequest = new PatchDeckRequest("updated_deck_name", null);
        var patchDeckResponse = await ApiClient.PatchAsJsonAsync($"decks/{deck!.Id}", patchDeckRequest);
        patchDeckResponse.Should().Be204NoContent();
        
        var updatedDeck = await ApiClient.GetFromJsonAsync<DeckDto>($"/decks/{deck.Id}");
        updatedDeck!.Name.Should().Be("updated_deck_name");
    }
    
    [Fact]
    public async Task Should_Make_Public_Successfully()
    {
        await AddDefaultAuthAsync();

        var createDeckRequest = new CreateDeckRequest("my_deck");
        var deck = await ApiClient.PostAsJsonAsync("decks", createDeckRequest).WaitForJson<Deck>();

        var patchDeckRequest = new PatchDeckRequest(null, true);
        var patchDeckResponse = await ApiClient.PatchAsJsonAsync($"decks/{deck!.Id}", patchDeckRequest);
        patchDeckResponse.Should().Be204NoContent();
        
        var updatedDeck = await ApiClient.GetFromJsonAsync<DeckDto>($"/decks/{deck.Id}");
        updatedDeck!.IsPublic.Should().BeTrue();
    }
    
    
    [Fact]
    public async Task Should_Be_Unauthorized()
    {
        var deckRequest = new PatchDeckRequest(null, true);
        var patchDeckResponse = await ApiClient.PatchAsJsonAsync($"decks/{Guid.NewGuid()}", deckRequest);
        patchDeckResponse.Should().Be401Unauthorized();
    }
    
    [Fact]
    public async Task Should_Return_BadRequest_When_Deck_Is_Created_By_Another_User()
    {
        using var anotherClient = await CreateUserClientAsync();
        var deckRequest = new CreateDeckRequest("my_deck");
        
        var createDeckResponse = await anotherClient.PostAsJsonAsync("decks", deckRequest);
        var createdDeck = await createDeckResponse.Content.ReadFromJsonAsync<Deck>();
        
        await AddDefaultAuthAsync();

        var patchDeckRequest = new PatchDeckRequest(null, true);
        var patchDeckResponse = await ApiClient.PatchAsJsonAsync($"decks/{createdDeck!.Id}", patchDeckRequest);
        patchDeckResponse.Should().Be400BadRequest();
        patchDeckResponse.Should().BeAs(new
        {
            status = 400,
            errors = new Dictionary<string, List<string>>()
            {
                {"", ["Could not find deck with the specified id"]}
            }
        });
    }
}