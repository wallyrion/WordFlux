using WordFlux.Web.Storage;

namespace WordFlux.Web;

public class WeatherApiClient(HttpClient httpClient, LocalStorage storage, ILogger<WeatherApiClient> logger)
{
    public async Task<WeatherForecast[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<WeatherForecast>? forecasts = null;

        await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<WeatherForecast>("/weatherforecast", cancellationToken))
        {
            if (forecasts?.Count >= maxItems)
            {
                break;
            }
            if (forecast is not null)
            {
                forecasts ??= [];
                forecasts.Add(forecast);
            }
        }

        return forecasts?.ToArray() ?? [];
    }
    
    
    public async Task<List<CardDto>> GetCards()
    {
        /*var requestUri = new UriBuilder()
        {
            Path = "/term",
            Query = $"term={term}"
        }.Uri;*/
        var myId = await storage.GetMyId();

        
        return (await httpClient.GetFromJsonAsync<List<CardDto>>($"/cards?userId={myId}"))!;
    }
    
    public async Task<string> GetMotivation()
    {
        var res = (await httpClient.GetFromJsonAsync<GetMotivationResult>($"/motivation"))!;

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
        
        var res = (await httpClient.GetFromJsonAsync<NextReviewCardTime>($"/cards/next/time?userId={myId}"))!;

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
    
    
    public async Task<List<TranslationItem>> GetTranslationExamples(string term, List<string> translations, string sourceLanguage, string destinationLanguage)
    {
        logger.LogInformation("Getting FROM UI examples for term {Term} and translations {@Translations}", term, translations);
        var request = new GetTranslationExamples(term, translations, sourceLanguage, destinationLanguage);
        var r = await httpClient.PostAsJsonAsync("/translations/examples", request);
        var r2 = await r.Content.ReadFromJsonAsync<List<TranslationItem>>();

        return r2!;
    }
    
    public async Task<List<string>> GetAlternatives(string term, List<string> translations, string sourceLanguage, string destinationLanguage)
    {
        var request = new GetTranslationExamples(term, translations, sourceLanguage, destinationLanguage);
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
    
    public async Task<TranslationResponse> GetTranslations(string term)
    {
        return (await httpClient.GetFromJsonAsync<TranslationResponse>($"/term?term={term}"))!;
    }
    
    public async Task<SimpleTranslationResult> GetSimpleTranslations(string term)
    {
        return (await httpClient.GetFromJsonAsync<SimpleTranslationResult>($"/translations?term={term}"))!;
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

public record WeatherForecast(Guid Id, DateTime CreatedAt, int TemperatureC);

public record TranslationResponse(string Term, List<TranslationItem> Translations, string Level, string? SuggestedTerm);
public record TranslationItem(string Term, string ExampleTranslated, string ExampleOriginal, int Popularity, string Level);
public record SimpleTranslationResult(string? SuggestedTerm, List<string> Translations, string SourceLanguage, string DestinationLanguage);

public record CardRequest(string Term, string Level, List<TranslationItem> Translations);
public record GetLevelResponse(string Level);
public record GetAudioLinkResponse(string Link);


public record CardDto(Guid Id, DateTime CreatedAt, string Term, string Level, List<TranslationItem> Translations, TimeSpan ReviewInterval);
public record NextReviewCardTime(TimeSpan TimeToNextReview);
public record GetMotivationResult(string Phrase);

public record GetTranslationExamples(string Term, List<string> Translations,  string SourceLanguage, string DestinationLanguage);
