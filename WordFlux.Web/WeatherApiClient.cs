using WordFlux.Web.Storage;

namespace WordFlux.Web;

public class WeatherApiClient(HttpClient httpClient, LocalStorage storage)
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
    
    public async Task<TranslationResponse> GetTranslations(string term)
    {
        /*var requestUri = new UriBuilder()
        {
            Path = "/term",
            Query = $"term={term}"
        }.Uri;*/
        return (await httpClient.GetFromJsonAsync<TranslationResponse>($"/term?term={term}"))!;
    }
    
    public async Task SaveNewCard(CardRequest card)
    {
        var myId = await storage.GetMyId();
        /*var requestUri = new UriBuilder()
        {
            Path = "/term",
            Query = $"term={term}"
        }.Uri;*/
        await httpClient.PostAsJsonAsync($"/cards?userId={myId}", card);
    }
}

public record WeatherForecast(Guid Id, DateTime CreatedAt, int TemperatureC);

public record TranslationResponse(string Term, List<TranslationItem> Translations, string? SuggestedTerm);
public record TranslationItem(string Term, string ExampleTranslated, string ExampleOriginal, int Popularity);

public record CardRequest(string Term, List<TranslationItem> Translations);


public record CardDto(Guid Id, DateTime CreatedAt, string Term, List<TranslationItem> Translations);