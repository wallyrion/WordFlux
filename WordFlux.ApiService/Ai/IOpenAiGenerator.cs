using WordFlux.Contracts;

namespace WordFlux.ApiService.Ai;

public interface IOpenAiGenerator
{
    Task<(string sourceLanguage, string destinationLanguage)?> DetectLanguage(string sourceInput, string translatedInput, CancellationToken cancellationToken = default);
    
    Task<List<(string ExampleLearn, string ExampleNative)>?> GetExamplesCardTask(string term, string learnLanguage, string nativeLanguage, int examplesCount,
        IReadOnlyList<string> translations, CancellationToken cancellationToken = default);
}


public interface IAzureAiTranslator
{
    Task<List<(string originalTerm, SimpleTranslationResponse translated)>> GetTranslations(List<string> terms, List<string> languages);
}