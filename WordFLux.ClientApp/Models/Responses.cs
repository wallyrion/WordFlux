namespace WordFLux.ClientApp.Models;

public record NextReviewCardTimeResponse(TimeSpan? TimeToNextReview);
public record GetMotivationResponse(string Phrase);

public record GetLevelResponse(string Level);
public record GetAudioLinkResponse(string Link);
