using WordFlux.ApiService.Ai;
using WordFlux.Contracts;

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
        }).CacheOutput();

        return app;
    }
}