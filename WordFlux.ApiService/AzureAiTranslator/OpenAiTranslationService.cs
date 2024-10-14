using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.AI.Translation.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WordFlux.ApiService.Services;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Ai;

#pragma warning disable SKEXP0010

public class AzureAiTranslationService : ITranslationService, IAzureAiTranslator

{
    private readonly OpenAiGenerator _aiGenerator;

    const string key = "";

    private readonly AzureKeyCredential _credential;
    private readonly TextTranslationClient client;
    private readonly ILogger<AzureAiTranslationService> _logger;
    
    public AzureAiTranslationService(OpenAiGenerator aiGenerator, IConfiguration configuration, ILogger<AzureAiTranslationService> logger)
    {
        _logger = logger;
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

    public async Task<IReadOnlyCollection<SupportedLanguage>> GetLanguagesAsync()
    {
        var languages = await client.GetSupportedLanguagesAsync();

        return languages.Value.Translation.Select(x => new SupportedLanguage(x.Value.Name, x.Value.NativeName, x.Key)).ToList();
    }
    
    public async Task<SimpleTranslationResponse?> GetTranslations(string term, List<string> languages, double? temperature)
    {
        Response<IReadOnlyList<TranslatedTextItem>> clientResult = await client.TranslateAsync(languages, [term]);

        if (!clientResult.HasValue || clientResult.Value.Count == 0)
        {
            return null;
        }

        var translationResult = clientResult.Value[0];
        var translations = translationResult.Translations.Where(x => x.TargetLanguage != translationResult.DetectedLanguage.Language).ToList();

        var sourceLanguage = translationResult.DetectedLanguage.Language;
        var targetLanguage = translations[0].TargetLanguage;
        
        var lookupResult = await GetDictionaryEntriesAsync(sourceLanguage, targetLanguage, term);

        var additionalTranslations = lookupResult.Take(5).ToList();

        var totalTranslations = translations.Select(x => x.Text).Concat(additionalTranslations).Distinct(new CaseInsensitiveValueComparer()).ToList();
        
        var response = new SimpleTranslationResponse(null, totalTranslations, sourceLanguage, targetLanguage);
        return response;

        //return await _aiGenerator.GetTranslations(term, languages);
    }
    
    public async Task<List<(string, SimpleTranslationResponse)>> GetTranslations(List<string> terms, List<string> languages)
    {
        Response<IReadOnlyList<TranslatedTextItem>> clientResult = await client.TranslateAsync(languages, terms);

        if (!clientResult.HasValue || clientResult.Value.Count == 0)
        {
            return [];
        }

        var results = clientResult.Value.Select(x =>
        {
            var translation = x.Translations.FirstOrDefault(y => y.TargetLanguage != x.DetectedLanguage.Language);

            var source = x.Translations.FirstOrDefault(y => y.TargetLanguage == x.DetectedLanguage.Language);

            if (source == null)
            {
                _logger.LogError("For some reason source of the translation was null that is not expected. Details: {@JsonDetails}",  x);
                return (null, null);
            }
            
            if (translation == null)
            {
                var destLanguage = x.DetectedLanguage.Language == languages[0] ? languages[1] : languages[0];
                return (source.Text, new SimpleTranslationResponse(null, [], x.DetectedLanguage.Language, destLanguage));
            }

            return (source.Text, new SimpleTranslationResponse(null, [translation.Text], x.DetectedLanguage.Language, translation.TargetLanguage));
        }).Where(x => x.Text != null).ToList();

        return results!;

        //return await _aiGenerator.GetTranslations(term, languages);
    }

    private async Task<IEnumerable<string>> GetDictionaryEntriesAsync(string sourceLanguage, string targetLanguage, string term)
    {
        if (sourceLanguage.Equals("en", StringComparison.OrdinalIgnoreCase) || targetLanguage.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            return (await client.LookupDictionaryEntriesAsync(sourceLanguage, targetLanguage, term)).Value[0].Translations.Select(x => x.DisplayTarget);
        }
        
        
        Response<IReadOnlyList<DictionaryLookupItem>> lookupResultToEnglish = await client.LookupDictionaryEntriesAsync(sourceLanguage, "en", term);
        var translations = lookupResultToEnglish.Value.SelectMany(x => x.Translations.Select(r => r.DisplayTarget)).ToList();
        
        var lookupResultToTarget = await client.LookupDictionaryEntriesAsync("en", targetLanguage, translations);

        return lookupResultToTarget.Value.SelectMany(x => x.Translations.Select(r => new { r.DisplayTarget, r.Confidence }))
            .OrderByDescending(x => x.Confidence)
            .Select(x => x.DisplayTarget);
    }  
    
    private async Task<(Response<IReadOnlyList<DictionaryExampleItem>>, bool isFullyTranslated)> GetDictionaryExamplesAsync(string sourceLanguage, string targetLanguage, string term, IEnumerable<string> inputTranslations)
    {
        if (sourceLanguage.Equals("en", StringComparison.OrdinalIgnoreCase) || targetLanguage.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            return (await client.LookupDictionaryExamplesAsync(sourceLanguage, targetLanguage, inputTranslations.Select(x => new InputTextWithTranslation(term, x))), true);
        }

        var termToEnglish = (await client.TranslateAsync("en", term, sourceLanguage)).Value[0].Translations[0].Text;

        var englishToTargetExamples = await client.LookupDictionaryExamplesAsync("en", targetLanguage, inputTranslations.Select(x => new InputTextWithTranslation(termToEnglish, x)));
        
  
        return (englishToTargetExamples, false);
        /*
        var lookupResultToTarget = await client.LookupDictionaryEntriesAsync("en", targetLanguage, translations);

        return lookupResultToTarget.Value.SelectMany(x => x.Translations.Select(r => new { r.DisplayTarget, r.Confidence }))
            .OrderByDescending(x => x.Confidence)
            .Select(x => x.DisplayTarget);*/
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
        var response = await GetDictionaryExamplesAsync(request.SourceLanguage, request.DestinationLanguage,  request.Term,  request.Translations);

        var items = response.Item1.Value.Select(x =>
            {
                var e = x.Examples.FirstOrDefault();

                if (e == null)
                {
                    return new TranslationItem(x.NormalizedTarget, "", "", 0, "Unknown");
                }

                string exampleOriginal = response.isFullyTranslated ? $"{e.SourcePrefix}*{e.SourceTerm}*{e.SourceSuffix}" : "";
                var exampleTranslated = $"{e.TargetPrefix}*{e.TargetTerm}*{e.TargetSuffix}";
                
            return new TranslationItem(x.NormalizedTarget, exampleTranslated, exampleOriginal, 0, "Unknown");
        })
        .ToList();

        return items;
    }
}