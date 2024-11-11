using WordFlux.Contracts;
using WordFlux.Domain.Domain;

namespace WordFlux.Application;

public interface ITranslationService
{
    Task<SimpleTranslationResponse?> GetTranslations(string term, List<string> languages, double? temperature = null);

    Task<List<TranslationItem>> GetExamples(GetTranslationExamplesRequest request);

    Task<IReadOnlyCollection<SupportedLanguage>> GetLanguagesAsync();
}