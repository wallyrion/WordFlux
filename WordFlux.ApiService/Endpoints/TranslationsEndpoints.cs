using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using WordFlux.ApiService.Ai;
using WordFlux.ApiService.Services;
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
        
        
        app.MapGet("/translations", async (string term, ITranslationService translation, string nativeLanguage, string studyingLanguage) =>
        {
            var response = await translation.GetTranslations(term, [nativeLanguage, studyingLanguage]);

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
        
        app.MapPost("/translations/examples", async (GetTranslationExamplesRequest request, ITranslationService translation) =>
        {
            var response = await translation.GetExamples(request);

            return Results.Ok(response);
        });
        
        
        return app;
        
        
    }
    
}