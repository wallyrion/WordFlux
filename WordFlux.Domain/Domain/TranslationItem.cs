namespace WordFlux.Domain.Domain;

public record TranslationItem(string Term, string ExampleTranslated, string ExampleOriginal, int Popularity, string Level);