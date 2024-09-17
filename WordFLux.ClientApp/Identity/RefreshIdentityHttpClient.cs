using System.Net.Http.Json;

namespace WordFLux.ClientApp.Identity;

public class RefreshIdentityHttpClient(HttpClient httpClient)
{
    public async Task<AuthResponse> RefreshToken(string refreshToken)
    {
        var response = await httpClient.PostAsJsonAsync("/refresh", new { refreshToken });
        
        response.EnsureSuccessStatusCode();
        
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }
}