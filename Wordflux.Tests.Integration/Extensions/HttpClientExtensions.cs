using System.Net.Http.Json;
using FluentAssertions;

namespace Wordflux.Tests.Integration.Extensions;

public static class HttpClientExtensions
{
    public static async Task<T?> WaitForJson<T>(this Task<HttpResponseMessage> httpResponseMessageTask,
        CancellationToken cancellationToken = default)
    {
        var response = await httpResponseMessageTask;
        response.Should().BeSuccessful();
        
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }
    
    public static async Task WaitForSuccess(this Task<HttpResponseMessage> httpResponseMessageTask)
    {
        var response = await httpResponseMessageTask;
        response.Should().BeSuccessful();
    }
}