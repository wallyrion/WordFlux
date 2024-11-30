using System.Net.Http.Headers;
using WordFLux.ClientApp.Services;

namespace WordFLux.ClientApp.Identity;

public class TokenHandler : DelegatingHandler
{

    private readonly TokenProvider _tokenProvider;
    private readonly ILogger<TokenHandler> _logger;
    private readonly RefreshIdentityHttpClient _refreshIdentityHttpClient;

    public TokenHandler(TokenProvider tokenProvider, ILogger<TokenHandler> logger, RefreshIdentityHttpClient refreshIdentityHttpClient)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
        _refreshIdentityHttpClient = refreshIdentityHttpClient;
    }

    /// <summary>
    /// Main method to override for the handler.
    /// </summary>
    /// <param name="request">The original request.</param>
    /// <param name="cancellationToken">The token to handle cancellations.</param>
    /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetAccessTokenAsync();
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var result = await base.SendAsync(request, cancellationToken);

        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshToken = await _tokenProvider.GetRefreshTokenAsync();

            if (refreshToken == null)
            {
                return result;
            }
            
            var authResponse = await _refreshIdentityHttpClient.RefreshToken(refreshToken!);
            if (authResponse == null)
            {
                return result;
            }
            
            await _tokenProvider.SetAuthTokensAsync(authResponse);
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
            result = await base.SendAsync(request, cancellationToken);
        }

        return result;

    }
}