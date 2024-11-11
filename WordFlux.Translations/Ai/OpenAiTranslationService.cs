using WordFlux.Application;
using WordFlux.Contracts;
using WordFlux.Domain.Domain;

namespace WordFlux.Translations.Ai;

#pragma warning disable SKEXP0010

public class OpenAiTranslationService(OpenAiGenerator aiGenerator) : ITranslationService
{
    private readonly OpenAiGenerator _aiGenerator = aiGenerator;

    public async Task<SimpleTranslationResponse?> GetTranslations(string term, List<string> languages, double? temperature = null)
    {
        var detectedLanguageResponse = await _aiGenerator.DetectLanguage(term, languages);

        var sourceLanguage = detectedLanguageResponse!.Value.sourceLanguage;
        var input = string.IsNullOrWhiteSpace(detectedLanguageResponse.Value.suggestedTerm) ? term : detectedLanguageResponse.Value.suggestedTerm;

        var translationsLanguage = languages.First(x => !string.Equals(x, sourceLanguage, StringComparison.InvariantCultureIgnoreCase));

        var translationsCount = input.Length > 30 ? 2 : 4;
        
        var translationsResponse = await _aiGenerator.GetTranslations(input, sourceLanguage, translationsLanguage, translationsCount, temperature);

        translationsResponse = translationsResponse! with { SourceLanguage = sourceLanguage, DestinationLanguage = translationsLanguage, SuggestedTerm = detectedLanguageResponse.Value.suggestedTerm};

        return translationsResponse;
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
    
        var response = await _aiGenerator.GetExamples(request.Term, request.Translations, request.SourceLanguage, request.DestinationLanguage);

        return response;
    }

    public async Task<IReadOnlyCollection<SupportedLanguage>> GetLanguagesAsync()
    {
        // not expected to use this method from this service. use Azure translation service
        throw new NotImplementedException();
    }
}