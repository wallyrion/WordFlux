using System.Diagnostics.CodeAnalysis;
using WordFlux.ApiService.Services;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Ai;

#pragma warning disable SKEXP0010

public class OpenAiTranslationService(OpenAiGenerator aiGenerator) : ITranslationService
{
    private readonly OpenAiGenerator _aiGenerator = aiGenerator;

    public async Task<SimpleTranslationResponse?> GetTranslations(string term, List<string> languages)
    {
        return await _aiGenerator.GetTranslations(term, languages);
    }

    public async Task<List<TranslationItem>> GetExamples(GetTranslationExamplesRequest request)
    {
        if (string.IsNullOrEmpty(request.SourceLanguage) || string.IsNullOrEmpty(request.DestinationLanguage))
        {
            (string srcLang, string destLang)? detectedLanguages = await _aiGenerator.DetectLanguage(request.Term, request.Translations.First());
        
            if (detectedLanguages == null)
            {
                throw new Exception("Could not detect languagee");
            }
        
            request = request with {SourceLanguage = detectedLanguages.Value.srcLang, DestinationLanguage = detectedLanguages.Value.destLang};
        }
    
        var response = await _aiGenerator.GetExamples(request.Term, request.Translations, request.SourceLanguage, request.DestinationLanguage);

        return response;
    }
}