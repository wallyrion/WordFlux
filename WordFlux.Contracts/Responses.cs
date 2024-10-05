namespace WordFlux.Contracts;

public record CardDto(Guid Id, DateTime CreatedAt, string Term, string Level, List<CardTranslationItem> Translations, TimeSpan ReviewInterval, string DeckName, string? ImageUrl = null, string? NativeLang = null, string? LearnLang = null, string? SourceLang = null, string? TargetLang = null);
public record SimpleTranslationResponse(string? SuggestedTerm, List<string> Translations, string SourceLanguage, string DestinationLanguage);
public record NextReviewCardTimeResponse(TimeSpan? TimeToNextReview);
public record GetMotivationResponse(string Phrase);

public record GetLevelResponse(string Level);
public record GetAudioLinkResponse(string Link);

public record AutocompleteResponse(string DetectedLanguage, List<AutocompleteItem> Items);
public record AutocompleteItem(string Term, string TermTranslated);
public record CreateDeckResponse(Guid Id, string Name);
public record ImportedDeckResponse(Guid Id, string Name, int ImportedCardsCount, List<string> FailedRawItems);

public record DeckDto(Guid Id, string Name, int CardsCount, DateTimeOffset CreatedAt, DeckType Type, bool IsPublic, bool IsEditable);

public record SupportedLanguage(string Name, string NativeName, string Code);
