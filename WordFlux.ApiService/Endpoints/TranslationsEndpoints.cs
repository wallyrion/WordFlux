using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using WordFlux.ApiService.Ai;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Endpoints;

#pragma warning disable SKEXP0010

public static class TranslationsEndpoints
{
    public static WebApplication MapTranslationEndpoints(this WebApplication app)
    {
        app.MapGet("/term/level", async (string term, OpenAiGenerator translation) =>
        {
            var respose = await translation.GetLevel(term);

            return new { Level = respose };
        });
        
        
        app.MapGet("/translations", async (string term, OpenAiGenerator translation) =>
        {
            var response = await translation.GetTranslations(term);

            if (response == null)
            {
                return Results.NotFound();
            }
            
            return Results.Ok(response);
        });
        
        app.MapPost("/translations/alternatives", async (OpenAiGenerator translation, GetTranslationExamplesRequest request) =>
        {
            var response = await translation.GetAlternativeTranslations(request.Term, request.SourceLanguage, request.DestinationLanguage, request.Translations);

            return response?.Translations ?? [];
        });
        
        app.MapPost("/translations/examples", async (GetTranslationExamplesRequest request, OpenAiGenerator ai) =>
        {
            if (string.IsNullOrEmpty(request.SourceLanguage) || string.IsNullOrEmpty(request.DestinationLanguage))
            {
                (string srcLang, string destLang)? detectedLanguages = await ai.DetectLanguage(request.Term, request.Translations.First());
        
                if (detectedLanguages == null)
                {
                    return Results.BadRequest("Could not detect languages");
                }
        
                request = request with {SourceLanguage = detectedLanguages.Value.srcLang, DestinationLanguage = detectedLanguages.Value.destLang};
            }
    
            var response = await ai.GetExamples(request.Term, request.Translations, request.SourceLanguage, request.DestinationLanguage);

            return Results.Ok(response);
        });
        
        
        return app;
        
        
    }
    
}