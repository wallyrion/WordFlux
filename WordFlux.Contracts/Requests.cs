namespace WordFlux.Contracts;

public record CardTranslationItem(string Term, string ExampleTranslated, string ExampleOriginal, int Popularity, string Level);

public record CardRequest(string Term, string Level, List<CardTranslationItem> Translations, Guid DeckId);


public record GetTranslationExamplesRequest(string Term, List<string> Translations, string SourceLanguage, string DestinationLanguage);
public record GetAutocompleteRequest(string Term, string SourceLanguage, string DestinationLanguage);
public record GetAutocompleteResponse(string DetectedLanguage, List<string> Completions);

public record CreateDeckRequest(string Name);
