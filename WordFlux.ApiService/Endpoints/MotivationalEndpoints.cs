using WordFlux.ApiService.Caching;
using WordFlux.Contracts;
using WordFlux.Translations.Ai;

namespace WordFlux.ApiService.Endpoints;

#pragma warning disable SKEXP0010

public static class MotivationalEndpoints
{
    public static WebApplication MapMotivationalEndpoints(this WebApplication app)
    {
        app.MapGet("/motivation", async (OpenAiGenerator translation) =>
        {
            var response = await translation.GetMotivationalPhrase();

            if (response == null)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            return Results.Ok(new GetMotivationResponse(response));
        }) .CacheOutput(p =>
        {
            p.AddPolicy<OutputCachePolicy>();
            p.Expire(TimeSpan.FromHours(1));
        });;

        return app;
    }
}