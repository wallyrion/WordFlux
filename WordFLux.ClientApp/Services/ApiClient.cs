using System.Net;
using System.Net.Http.Json;
using WordFLux.ClientApp.Models;
using WordFLux.ClientApp.Storage;
using WordFlux.Contracts;
using GetAudioLinkResponse = WordFLux.ClientApp.Models.GetAudioLinkResponse;
using GetLevelResponse = WordFLux.ClientApp.Models.GetLevelResponse;
using GetMotivationResponse = WordFLux.ClientApp.Models.GetMotivationResponse;
using GetTranslationExamplesRequest = WordFLux.ClientApp.Models.GetTranslationExamplesRequest;
using NextReviewCardTimeResponse = WordFLux.ClientApp.Models.NextReviewCardTimeResponse;

namespace WordFLux.ClientApp.Services;

public class WeatherApiClient(HttpClient httpClient, LocalStorage storage, ILogger<WeatherApiClient> logger)
{
    public async Task<NotificationSubscription> SubscribeToNotification(NotificationSubscription subscription)
    {
        var response = await httpClient.PostAsJsonAsync("/notifications", subscription);
        var notificationSubscription = await response.Content.ReadFromJsonAsync<NotificationSubscription>();

        return notificationSubscription!;
    } 
    
    public async Task UnsubscribeFromNotifications(Guid subscriptionId)
    {
        await httpClient.DeleteAsync($"/notifications/{subscriptionId}");
    }

    public async Task<NotificationSubscription?> GetNotificationSubscriptionByUrl(string url)
    {
        var response = await httpClient.PostAsJsonAsync($"/notifications-by-url", new { Url = url });
        
        var notificationSubscription = await response.Content.ReadFromJsonAsync<NotificationSubscription>();
        
        return notificationSubscription;
    }
    
    public async Task<List<CardDto>> GetCards(Guid? deckId)
    {
        return (await httpClient.GetFromJsonAsync<List<CardDto>>($"/cards?deckId={deckId}"))!;
    }    
    public async Task<CardDto?> GetCard(Guid cardId)
    {
        var card = await httpClient.GetFromJsonAsync<CardDto>($"/cards/{cardId}");

        return card;
    }
    
    public async Task<string> GetMotivation()
    {
        var res = (await httpClient.GetFromJsonAsync<GetMotivationResponse>($"/motivation"))!;

        return res.Phrase;
    }
    
    public async Task<CardDto> GetNextCard(List<Guid> selectedDecksIds, int skip = 0)
    {
        
        var decksParam = selectedDecksIds.Count > 0 ? $"&deckIds={string.Join(",", selectedDecksIds)}" : "";
        
        return (await httpClient.GetFromJsonAsync<CardDto>($"/cards/next?skip={skip}{decksParam}"))!;
    }
    
    public async Task<TimeSpan?> GetNextReviewTime(List<Guid> selectedDecksIds)
    {
        var decksParam = selectedDecksIds.Count > 0 ? $"?deckIds={string.Join(",", selectedDecksIds)}" : "";

        var res = (await httpClient.GetFromJsonAsync<NextReviewCardTimeResponse>($"/cards/next/time{decksParam}"))!;

        return res.TimeToNextReview;
    }
    
    public async Task<GetAutocompleteResponse> SearchForCompletions(string term)
    {
        var request = new GetAutocompleteRequest(term, "ru", "en");
        
        var response = await httpClient.PostAsJsonAsync($"/translations/autocomplete", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<GetAutocompleteResponse>())!;
    }
    
    public async Task<AutocompleteResponse> SearchForCompletionsWithTranslations(string term, CancellationToken token)
    {
        var request = new GetAutocompleteRequest(term, "ru", "en");
        
        var response = await httpClient.PostAsJsonAsync($"/translations/autocomplete/with-translations", request, cancellationToken: token);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AutocompleteResponse>(cancellationToken: token))!;
    }
    
    
    public async Task ApproveCard(Guid cardId)
    {
        var myId = await storage.GetMyId();
        
        await httpClient.PostAsync($"/cards/{cardId}/approve?userId={myId}", null!);
    }
    
    public async Task RejectCard(Guid cardId)
    {
        var myId = await storage.GetMyId();
        
        await httpClient.PostAsync($"/cards/{cardId}/reject?userId={myId}", null!);
    }
    
    
    public async Task<List<CardTranslationItem>> GetTranslationExamples(string term, List<string> translations, string sourceLanguage, string destinationLanguage,
        bool useAzureAiTranslator, CancellationToken token = default)
    {
        logger.LogInformation("Getting FROM UI examples for term {Term} and translations {@Translations}", term, translations);
        var request = new GetTranslationExamplesRequest(term, translations, sourceLanguage, destinationLanguage);
        var response = await httpClient.PostAsJsonAsync($"/translations/examples?useAzureAiTranslator={useAzureAiTranslator}", request, cancellationToken: token);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CardTranslationItem>>(cancellationToken: token);

        return result!;
    }
    
    public async Task<List<string>> GetAlternatives(string term, List<string> translations, string sourceLanguage, string destinationLanguage)
    {
        var request = new GetTranslationExamplesRequest(term, translations, sourceLanguage, destinationLanguage);
        var r = await httpClient.PostAsJsonAsync("/translations/alternatives", request);
        var r2 = await r.Content.ReadFromJsonAsync<List<string>>();

        return r2!;
    }
    
    
    public async Task<byte[]> GetAudio(string term)
    {
        // get audio from the server
        
        
        var response =  (await httpClient.GetAsync(($"/audio?term={term}")));

        var bytes = await response.Content.ReadAsByteArrayAsync();
        
        return bytes;
    }
    
    public async Task<string> GetAudioLink(string term)
    {
        // get audio from the server
        var response =  (await httpClient.GetFromJsonAsync<GetAudioLinkResponse>(($"/audio/link?term={term}")));

        return response.Link;
    }
    
    
    public async Task<SimpleTranslationResponse> GetSimpleTranslations(string term, bool useAzureAiTranslator, CancellationToken token)
    {
        var languages = await storage.GetMyLanguages();
        
        return (await httpClient.GetFromJsonAsync<SimpleTranslationResponse>($"/translations?term={term}&nativeLanguage={languages.native}&studyingLanguage={languages.studing}&useAzureAiTranslator={useAzureAiTranslator}", cancellationToken: token))!;
    }
    public async Task<string> GetLevel(string term, CancellationToken token)
    {
        var res =  (await httpClient.GetFromJsonAsync<GetLevelResponse>($"/term/level?term={term}", cancellationToken: token))!;

        return res.Level;
    }
    
    public async Task SaveNewCard(CardRequest card)
    {
        await httpClient.PostAsJsonAsync($"/cards", card);
    }
    
    public async Task UpdateCard(CardRequest card, Guid cardId)
    {
        await httpClient.PutAsJsonAsync($"/cards/{cardId}", card);
    }
    
    public async Task RemoveCard(Guid cardId)
    {
        var myId = await storage.GetMyId();
        
        await httpClient.DeleteAsync($"/cards/{cardId}?userId={myId}");
    }

    public async Task<List<DeckDto>> GetDecks()
    {
        return (await httpClient.GetFromJsonAsync<List<DeckDto>>($"/decks"))!;
    }
    public async Task<DeckDto> GetDeck(Guid deckId)
    {
        return (await httpClient.GetFromJsonAsync<DeckDto>($"/decks/{deckId}"))!;
    }
    public async Task PatchDeck(Guid deckId, string? name = null, bool? isPublic = null)
    {
        await httpClient.PatchAsJsonAsync($"/decks/{deckId}", new { name, isPublic  });
    }
    
    public async Task<CreateDeckResponse> CreateDeck(string newName)
    {
        var response = await httpClient.PostAsJsonAsync($"/decks", new { Name = newName });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateDeckResponse>();

        return result!;
    }
    public async Task<CreateDeckResponse> DuplicateDeck(Guid deckId, string newName)
    {
        var response = await httpClient.PostAsJsonAsync($"/decks/{deckId}/duplicate?duplicateName={newName}", new {});
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateDeckResponse>();

        return result!;
    }
    
    public async Task RemoveDeck(Guid id)
    {
        var response = await httpClient.DeleteAsync($"/decks/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<ImportedDeckResponse?> ImportDeck(string? importText)
    {
        var escapedStr = Uri.EscapeDataString(importText);
        
        var response = await httpClient.PostAsJsonAsync($"/decks/import", new { Cards = escapedStr });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ImportedDeckResponse>();

        return result;
    }
}