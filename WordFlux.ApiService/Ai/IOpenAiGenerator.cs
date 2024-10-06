namespace WordFlux.ApiService.Ai;

public interface IOpenAiGenerator
{
    Task<(string sourceLanguage, string destinationLanguage)?> DetectLanguage(string sourceInput, string translatedInput, CancellationToken cancellationToken = default);
}