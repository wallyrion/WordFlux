using DeepL;
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
        
        
        app.MapGet("/translations", async (string term, IServiceProvider di, string nativeLanguage, string studyingLanguage, bool useAzureAiTranslator) =>
        {
            var translationService = di.ResolveTranslationService(useAzureAiTranslator);
            var response = await translationService.GetTranslations(term, [nativeLanguage, studyingLanguage]);

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
        
        app.MapPost("/translations/examples", async (GetTranslationExamplesRequest request, IServiceProvider di, bool useAzureAiTranslator) =>
        {
            var translationService = di.ResolveTranslationService(useAzureAiTranslator);

            var response = await translationService.GetExamples(request);

            return Results.Ok(response);
        });
        
        
        app.MapPost("/translations/autocomplete/with-translations", async (GetAutocompleteRequest request, OpenAiGenerator openAiGenerator) =>
        {
            var result = await openAiGenerator.GetAutocompleteWithTranslations(request.Term, request.SourceLanguage, request.DestinationLanguage);

            var items = result.Value.autocompletes.Select(x => new AutocompleteItem(x.Item1, x.Item2)).ToList();
            var response = new AutocompleteResponse(result.Value.detectedLanguage, items);

            return response;
        }).CacheOutput(t => t.Expire(TimeSpan.FromMinutes(1)));
        
        app.MapPost("/translations/autocomplete", async (GetAutocompleteRequest request, OpenAiGenerator openAiGenerator) =>
        {
            var result = await openAiGenerator.GetAutocomplete(request.Term, request.SourceLanguage, request.DestinationLanguage);

            return new GetAutocompleteResponse (result.Value.detectedLanguage, result.Value.autocompletes);
        });
        
        
        app.MapPost("/translations/deepl", async (GetAutocompleteRequest request, IConfiguration configuration) =>
        {
            
            var authKey = configuration["DeeplAuthKey"]; // Replace with your key
            var translator = new Translator(authKey);
            
            var translatedText = await translator.TranslateTextAsync(
                request.Term,
                LanguageCode.English,
                LanguageCode.Russian);

            return translatedText.Text;
        });
        
        
        return app;
        
        
    }
    
}