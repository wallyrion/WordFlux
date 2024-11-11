using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace WordFlux.Infrastructure.ImageSearch;

public class BingImageSearchService(IConfiguration config)
{
    private const string BaseUrl = "https://api.bing.microsoft.com";
    private readonly string _apiKey = config["BingSearchApiKey"]!;
    
    public async Task<List<string>> GetImagesByKeyword(string keyword)
    {
        //var result = JsonSerializer.Deserialize<ImageSearchResponse>(Constants.TemporaryImageSearchResponse);

        //return result.Value.Select(x => x.ContentUrl).ToList();
        
        
        using var client = new HttpClient();
        
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

        var response = await client.GetAsync($"/v7.0/images/search?q={keyword}");
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadFromJsonAsync<ImageSearchResponse>();

        return content.Value.Select(x => x.ContentUrl).ToList();
    }
    
   
}


public class ImageSearchResponse
{
    public List<ImageSearchValues> Value { get; set; } = [];
}


public class ImageSearchValues
{
    public string ContentUrl { get; set; }
}