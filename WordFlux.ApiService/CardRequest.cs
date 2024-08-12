namespace WordFlux.ApiService;

public record CardRequest(string Term, string Level, List<CardTranslationItem> Translations);