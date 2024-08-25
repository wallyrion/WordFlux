namespace WordFlux.ApiService;

public record TranslationItem(string Term, string ExampleTranslated, string ExampleOriginal, int Popularity, string Level);