using System.Net.Http.Json;
using WordFlux.Contracts;

namespace WordFLux.ClientApp.Identity;

public class RefreshIdentityHttpClient(HttpClient httpClient)
{
    public async Task<AuthResponse?> RefreshToken(string refreshToken)
    {
        var response = await httpClient.PostAsJsonAsync("/refresh", new { refreshToken });

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }
}