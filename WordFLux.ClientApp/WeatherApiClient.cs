using System.Net.Http.Json;
using WordFLux.ClientApp.Models;
using WordFlux.Contracts;
using WordFlux.Web.Storage;

namespace WordFlux.Web;

public class WeatherApiClient(HttpClient httpClient, LocalStorage storage, ILogger<WeatherApiClient> logger)
{
    public async Task<List<CardDto>> GetCards()
    {
        var myId = await storage.GetMyId();

        return (await httpClient.GetFromJsonAsync<List<CardDto>>($"/cards?userId={myId}"))!;
    }
    
    public async Task<string> GetMotivation()
    {
        var res = (await httpClient.GetFromJsonAsync<GetMotivationResponse>($"/motivation"))!;

        return res.Phrase;
    }
    
    public async Task<CardDto> GetNextCard(int skip = 0)
    {
        var myId = await storage.GetMyId();
        
        return (await httpClient.GetFromJsonAsync<CardDto>($"/cards/next?userId={myId}&skip={skip}"))!;
    }
    
    public async Task<TimeSpan> GetNextReviewTime()
    {
        var myId = await storage.GetMyId();
        
        var res = (await httpClient.GetFromJsonAsync<NextReviewCardTimeResponse>($"/cards/next/time?userId={myId}"))!;

        return res.TimeToNextReview;
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
    
    
    public async Task<List<CardTranslationItem>> GetTranslationExamples(string term, List<string> translations, string sourceLanguage, string destinationLanguage)
    {
        logger.LogInformation("Getting FROM UI examples for term {Term} and translations {@Translations}", term, translations);
        var request = new GetTranslationExamplesRequest(term, translations, sourceLanguage, destinationLanguage);
        var response = await httpClient.PostAsJsonAsync("/translations/examples", request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CardTranslationItem>>();

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
    
    
    public async Task<SimpleTranslationResponse> GetSimpleTranslations(string term)
    {
        var languages = await storage.GetMyLanguages();
        
        return (await httpClient.GetFromJsonAsync<SimpleTranslationResponse>($"/translations?term={term}&nativeLanguage={languages.native}&studyingLanguage={languages.studing}"))!;
    }
    public async Task<string> GetLevel(string term)
    {
        var res =  (await httpClient.GetFromJsonAsync<GetLevelResponse>($"/term/level?term={term}"))!;

        return res.Level;
    }
    
    public async Task SaveNewCard(CardRequest card)
    {
        var myId = await storage.GetMyId();

        await httpClient.PostAsJsonAsync($"/cards?userId={myId}", card);
    }
    
    public async Task UpdateCard(CardRequest card, Guid cardId)
    {
        var myId = await storage.GetMyId();
        
        await httpClient.PutAsJsonAsync($"/cards/{cardId}?userId={myId}", card);
    }
    
    public async Task DeleteCard(Guid cardId)
    {
        var myId = await storage.GetMyId();
        
        await httpClient.DeleteAsync($"/cards/{cardId}?userId={myId}");
    }
}