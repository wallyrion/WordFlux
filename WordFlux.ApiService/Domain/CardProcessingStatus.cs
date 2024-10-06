namespace WordFlux.ApiService.Domain;

public enum CardProcessingStatus
{
    Unprocessed,
    LanguageDetected,
    CardExampleTaskCreated,
    Failed = 100
}