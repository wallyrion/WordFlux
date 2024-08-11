namespace WordFlux.ApiService;

public record CardRequest(string Term, List<CardTranslationItem> Translations);