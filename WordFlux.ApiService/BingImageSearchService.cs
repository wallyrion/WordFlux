using System.Text.Json;

namespace WordFlux.ApiService;

public class BingImageSearchService
{
    const string baseUrl = "https://api.bing.microsoft.com";
    const string apiKey = "";
    
    public async Task<List<string>> GetImagesByKeyword(string keyword)
    {
        var result = JsonSerializer.Deserialize<ImageSearchResponse>(Constants.TemporaryImageSearchResponse);

        return result.Value.Select(x => x.ContentUrl).ToList();
        
        
        using var client = new HttpClient();
        
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

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