namespace WordFlux.Contracts;

public record CardTranslationItem(string Term, string? ExampleTranslated = null, string? ExampleOriginal = null, int Popularity = 0, string? Level = null);

public record CardRequest(string Term, string Level, List<CardTranslationItem> Translations, Guid DeckId = default);


public record GetTranslationExamplesRequest(string Term, List<string> Translations, string SourceLanguage, string DestinationLanguage);
public record GetAutocompleteRequest(string Term, string SourceLanguage, string DestinationLanguage);
public record GetAutocompleteResponse(string DetectedLanguage, List<string> Completions);

public record CreateDeckRequest(string Name);
public record PatchDeckRequest(string? Name, bool? IsPublic);

public record ImportDeckRequest(string? Name, string Cards);