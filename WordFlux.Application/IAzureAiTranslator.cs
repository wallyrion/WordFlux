using WordFlux.Contracts;

namespace WordFlux.Application;

public interface IAzureAiTranslator
{
    Task<List<(string originalTerm, SimpleTranslationResponse translated)>> GetTranslations(List<string> terms, List<string> languages);
}