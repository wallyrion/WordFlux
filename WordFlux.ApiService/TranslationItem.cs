namespace WordFlux.ApiService;

public record TranslationItem(string Term, string ExampleTranslated, string ExampleOriginal, int Popularity, string Level);
public record SimpleTranslationResult(string? SuggestedTerm, List<string> Translations, string SourceLanguage, string DestinationLanguage);