namespace WordFlux.ApiService;

public record TranslationResponse(string Term, IEnumerable<TranslationItem> Translations, string? SuggestedTerm);