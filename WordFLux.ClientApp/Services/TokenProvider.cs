using Blazored.LocalStorage;
using WordFLux.ClientApp.Identity;
using WordFLux.ClientApp.Storage;

namespace WordFLux.ClientApp.Services;

public class TokenProvider(ILocalStorageService localStorage)
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    
    public async Task SetAuthTokensAsync(AuthResponse auth)
    {
        await localStorage.SetItemAsStringAsync(AccessTokenKey, auth.AccessToken);
        await localStorage.SetItemAsStringAsync(RefreshTokenKey, auth.RefreshToken);
    }
    
    public async Task ClearTokensAsync()
    {
        await localStorage.RemoveItemsAsync([AccessTokenKey, RefreshTokenKey]);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await localStorage.GetItemAsStringAsync(AccessTokenKey);
    }
    public async Task<string?> GetRefreshTokenAsync()
    {
        return await localStorage.GetItemAsStringAsync(RefreshTokenKey);
    }
}