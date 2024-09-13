using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.AI.Translation.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WordFlux.ApiService.Services;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Ai;

#pragma warning disable SKEXP0010

public class AzureAiTranslationService : ITranslationService

{
    private readonly OpenAiGenerator _aiGenerator;

    const string key = "";

    private readonly AzureKeyCredential _credential;
    private readonly TextTranslationClient client;

    public AzureAiTranslationService(OpenAiGenerator aiGenerator, IConfiguration configuration)
    {
        _aiGenerator = aiGenerator;
        var azureAiKey = configuration["AzureAiTranslatorKey"];
        _credential = new(azureAiKey);
        
        client  = new(_credential, "westeurope");
    }

    
    /*public async Task<SimpleTranslationResponse?> GetTranslations(string term, List<string> languages)
    {
        Response<IReadOnlyList<TranslatedTextItem>> clientResult = await client.TranslateAsync(["en", "ru"], [term]);

        if (!clientResult.HasValue || clientResult.Value.Count == 0)
        {
            return null;
        }

        var translationResult = clientResult.Value[0];
        var translations = translationResult.Translations.Where(x => x.TargetLanguage != translationResult.DetectedLanguage.Language).ToList();

        var sourceLanguage = translationResult.DetectedLanguage.Language;
        var targetLanguage = translations[0].TargetLanguage;
        
        Response<IReadOnlyList<DictionaryLookupItem>> lookupResult = await client.LookupDictionaryEntriesAsync(sourceLanguage, targetLanguage, term);

        var additionalTranslations = lookupResult.Value[0].Translations.Select(x => x.DisplayTarget).Take(5).ToList();

        var totalTranslations = translations.Select(x => x.Text).Concat(additionalTranslations).Distinct(new CaseInsensitiveValueComparer()).ToList();
        
        var response = new SimpleTranslationResponse(null, totalTranslations, sourceLanguage, targetLanguage);
        return response;

        //return await _aiGenerator.GetTranslations(term, languages);
    }*/
    
    public async Task<SimpleTranslationResponse?> GetTranslations(string term, List<string> languages)
    {
        Response<IReadOnlyList<TranslatedTextItem>> clientResult = await client.TranslateAsync(["en", "ru"], [term]);

        if (!clientResult.HasValue || clientResult.Value.Count == 0)
        {
            return null;
        }

        var translationResult = clientResult.Value[0];
        var translations = translationResult.Translations.Where(x => x.TargetLanguage != translationResult.DetectedLanguage.Language).ToList();

        var sourceLanguage = translationResult.DetectedLanguage.Language;
        var targetLanguage = translations[0].TargetLanguage;
        
        Response<IReadOnlyList<DictionaryLookupItem>> lookupResult = await client.LookupDictionaryEntriesAsync(sourceLanguage, targetLanguage, term);

        var additionalTranslations = lookupResult.Value[0].Translations.Select(x => x.DisplayTarget).Take(5).ToList();

        var totalTranslations = translations.Select(x => x.Text).Concat(additionalTranslations).Distinct(new CaseInsensitiveValueComparer()).ToList();
        
        var response = new SimpleTranslationResponse(null, totalTranslations, sourceLanguage, targetLanguage);
        return response;

        //return await _aiGenerator.GetTranslations(term, languages);
    }

    public async Task<List<TranslationItem>> GetExamples(GetTranslationExamplesRequest request)
    {
        if (string.IsNullOrEmpty(request.SourceLanguage) || string.IsNullOrEmpty(request.DestinationLanguage))
        {
            (string srcLang, string destLang)? detectedLanguages = await _aiGenerator.DetectLanguages(request.Term, request.Translations.First());
        
            if (detectedLanguages == null)
            {
                throw new Exception("Could not detect languagee");
            }
        
            request = request with {SourceLanguage = detectedLanguages.Value.srcLang, DestinationLanguage = detectedLanguages.Value.destLang};
        }

        var translations = request.Translations.Select(x => new InputTextWithTranslation(request.Term, x));
        Response<IReadOnlyList<DictionaryExampleItem>> response = await client.LookupDictionaryExamplesAsync(request.SourceLanguage, request.DestinationLanguage, translations);

        var items = response.Value.Select(x =>
            {
                var e = x.Examples.FirstOrDefault();

                if (e == null)
                {
                    return new TranslationItem(x.NormalizedTarget, "", "", 0, "Unknown");
                }
                    
                var exampleOriginal = $"{e.SourcePrefix}*{e.SourceTerm}*{e.SourceSuffix}";
                var exampleTranslated = $"{e.TargetPrefix}*{e.TargetTerm}*{e.TargetSuffix}";
                
            return new TranslationItem(x.NormalizedTarget, exampleTranslated, exampleOriginal, 0, "Unknown");
        })
        .ToList();

        return items;
    }
}