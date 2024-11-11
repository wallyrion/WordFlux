using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace WordFlux.Infrastructure.ImageSearch;

public class UnsplashImageSearchService(IConfiguration configuration)
{
    const string baseUrl = "https://api.unsplash.com/";
    
    public async Task<List<string>> GetImagesByKeyword(string keyword)
    {
        var apiKey = configuration["UnsplashApiKey"];
        
        using var client = new HttpClient();
        
        client.BaseAddress = new Uri(baseUrl);

        var url = $"/search/photos?client_id={apiKey}&query={keyword}";

        var content = await client.GetAsync(url);

        var response = await content.Content.ReadFromJsonAsync<Response>();
        
        //var response = await client.GetFromJsonAsync<Response>(url);

        return response.Results.Select(r => r.Urls.Regular).ToList();
    }
    
   
}


file class Response
{
    public List<ResultItem> Results { get; set; }   
}

file class ResultItem
{
    public UrlItem Urls { get; set; }
}

file class UrlItem
{
    [JsonPropertyName("regular")]
    public string Regular { get; set; }
}


