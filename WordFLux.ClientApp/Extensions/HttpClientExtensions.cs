using System.Net.Http.Headers;

namespace WordFLux.ClientApp.Extensions;

public static class DependencyInjection
{
    public static IHttpClientBuilder AddDefaultApiClient<T>(this IServiceCollection builder) where T : class
    {
        builder.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
        });

        return builder.AddHttpClient<T>((provider, client) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var url = configuration["BackendUrl"];
            client.BaseAddress = new Uri(url!);

        });
    }
}