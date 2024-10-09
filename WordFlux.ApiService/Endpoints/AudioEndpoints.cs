using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using WordFlux.ApiService.Ai;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Endpoints;

public static class AudioEndpoints
{
    public static WebApplication MapAudioEndpoints(this WebApplication app)
    {
        app.MapGet("/audio/link", (string term) =>
        {
            const string url = "https://wordflux-api.azurewebsites.net";

            return new GetAudioLinkResponse($"{url}/audio?term={Uri.EscapeDataString(term)}");
        }).CacheOutput();

#pragma warning disable SKEXP0001
        app.MapGet("/audio", async ([FromServices] IAudioAiGenerator audioGenerator, string term, CancellationToken cancellationToken = default) =>
        {
            var audioContent = await audioGenerator.GenerateAudioFromTextAsync(term, cancellationToken);
            return Results.File(audioContent, "audio/mp3");
        }).WithName("audio").CacheOutput();

        return app;
    }
}