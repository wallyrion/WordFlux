namespace WordFlux.ApiService;

public class ImageSearchService
{
    const string baseUrl = "https://api.bing.microsoft.com";
    const string apiKey = "";
    
    public async Task<ImageSearchResponse?> GetImagesByKeyword(string keyword)
    {
        using var client = new HttpClient();
        
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

        var response = await client.GetAsync($"/v7.0/images/search?q={keyword}");
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadFromJsonAsync<ImageSearchResponse>();

        return content;
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