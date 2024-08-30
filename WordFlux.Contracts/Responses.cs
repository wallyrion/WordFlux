namespace WordFlux.Contracts;

public record SimpleTranslationResponse(string? SuggestedTerm, List<string> Translations, string SourceLanguage, string DestinationLanguage);
public record NextReviewCardTimeResponse(TimeSpan? TimeToNextReview);
public record GetMotivationResponse(string Phrase);

public record GetLevelResponse(string Level);
public record GetAudioLinkResponse(string Link);
