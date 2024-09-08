
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using WordFLux.ClientApp.Services;

namespace WordFLux.ClientApp.Identity;

/// <summary>
/// Handler to ensure cookie credentials are automatically sent over with each request.
/// </summary>
public class CookieHandler : DelegatingHandler
{
    /// <summary>
    /// Main method to override for the handler.
    /// </summary>
    /// <param name="request">The original request.</param>
    /// <param name="cancellationToken">The token to handle cancellations.</param>
    /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // include cookies!
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Add("X-Requested-With", ["XMLHttpRequest"]);

        return base.SendAsync(request, cancellationToken);
    }
}



public class TokenHandler : DelegatingHandler
{

    private readonly TokenProvider _tokenProvider;

    public TokenHandler(TokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
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

        return result;

    }
}