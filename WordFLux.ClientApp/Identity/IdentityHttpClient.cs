using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using WordFLux.ClientApp.Identity.Models;
using WordFLux.ClientApp.Services;
using WordFlux.Contracts;

namespace WordFLux.ClientApp.Identity;

public class IdentityHttpClient(HttpClient httpClient, TokenProvider tokenProvider)
{
    private readonly JsonSerializerOptions jsonSerializerOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

    public async Task<FormResult> RegisterAsync(string email, string password)
    {
        string[] defaultDetail = ["An unknown error prevented registration from succeeding."];

        string userName = "Oleksii Korniienko";

        try
        {
            // make the request
            var result = await httpClient.PostAsJsonAsync(
                "register", new
                {
                    email,
                    password
                });

            // successful?
            if (result.IsSuccessStatusCode)
            {
                return new FormResult { Succeeded = true };
            }

            // body should contain details about why it failed
            var details = await result.Content.ReadAsStringAsync();
            var problemDetails = JsonDocument.Parse(details);
            var errors = new List<string>();
            var errorList = problemDetails.RootElement.GetProperty("errors");

            foreach (var errorEntry in errorList.EnumerateObject())
            {
                if (errorEntry.Value.ValueKind == JsonValueKind.String)
                {
                    errors.Add(errorEntry.Value.GetString()!);
                }
                else if (errorEntry.Value.ValueKind == JsonValueKind.Array)
                {
                    errors.AddRange(
                        errorEntry.Value.EnumerateArray().Select(
                                e => e.GetString() ?? string.Empty)
                            .Where(e => !string.IsNullOrEmpty(e)));
                }
            }

            // return the error list
            return new FormResult
            {
                Succeeded = false,
                ErrorList = problemDetails == null ? defaultDetail : [.. errors]
            };
        }
        catch
        {
        }

        // unknown error
        return new FormResult
        {
            Succeeded = false,
            ErrorList = defaultDetail
        };
    }

    public async Task<UserInfo> GetUserInfoAsync()
    {
        // the user info endpoint is secured, so if the user isn't logged in this will fail
        var userResponse = await httpClient.GetAsync("manage/info");

        // throw if user info wasn't retrieved
        userResponse.EnsureSuccessStatusCode();

        // user is authenticated,so let's build their authenticated identity
        var userInfo = await userResponse.Content.ReadFromJsonAsync<UserInfo>();
        return userInfo!;
    }

    
    public async Task<RoleClaim[]> GetRolesAsync()
    {
        // the user info endpoint is secured, so if the user isn't logged in this will fail
        var userResponse = await httpClient.GetAsync("roles");

        // throw if user info wasn't retrieved
        userResponse.EnsureSuccessStatusCode();

        // user is authenticated,so let's build their authenticated identity
        var roles = await userResponse.Content.ReadFromJsonAsync<RoleClaim[]>();
        return roles!;
    }
    
    
    public async Task LogoutAsync()
    {
        await tokenProvider.ClearTokensAsync();
        await httpClient.PostAsJsonAsync("logout", new {});
    }

    
    public async Task<FormResult> LoginAsync(string email, string password)
    {
        try
        {
            // login with cookies
            var result = await httpClient.PostAsJsonAsync(
                "login", new
                {
                    email,
                    password
                });

            // success?
            if (result.IsSuccessStatusCode)
            {
                var authResponse = await result.Content.ReadFromJsonAsync<AuthResponse>();
                await tokenProvider.SetAuthTokensAsync(authResponse!);

                // success!
                return new FormResult { Succeeded = true };
            }
        }
        catch
        {
        }

        // unknown error
        return new FormResult
        {
            Succeeded = false,
            ErrorList = ["Invalid email and/or password."]
        };
    }
}

public class RoleClaim
{
    public string? Issuer { get; set; }
    public string? OriginalIssuer { get; set; }
    public string? Type { get; set; }
    public string? Value { get; set; }
    public string? ValueType { get; set; }
}