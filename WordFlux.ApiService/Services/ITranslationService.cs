using WordFlux.Contracts;

namespace WordFlux.ApiService.Services;

public interface ITranslationService
{
    Task<SimpleTranslationResponse?> GetTranslations(string term, List<string> languages);

    Task<List<TranslationItem>> GetExamples(GetTranslationExamplesRequest request);
}