using WordFlux.Contracts;

namespace WordFlux.Web;

public record CardDto(Guid Id, DateTime CreatedAt, string Term, string Level, List<CardTranslationItem> Translations, TimeSpan ReviewInterval);