namespace WordFLux.ClientApp.Models;

public record CardDto(Guid Id, DateTime CreatedAt, string Term, string Level, List<CardTranslationItem> Translations, TimeSpan ReviewInterval);