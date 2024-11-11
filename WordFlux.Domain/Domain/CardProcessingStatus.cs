namespace WordFlux.Domain.Domain;

public enum CardProcessingStatus
{
    Unprocessed,
    LanguageDetected,
    CardExampleTaskCreated,
    Failed = 100
}