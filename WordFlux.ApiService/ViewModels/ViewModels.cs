namespace WordFlux.ApiService.ViewModels;

public record CardTranslationItemRequest(string Term, string? ExampleTranslated = null, string? ExampleOriginal = null, int Popularity = 0, string? Level = null);