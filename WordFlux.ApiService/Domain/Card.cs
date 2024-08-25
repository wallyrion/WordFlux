using WordFlux.Contracts;

namespace WordFlux.ApiService.Domain;

public class Card
{
    public Guid Id { get;  set; }
    public DateTime CreatedAt { get; set; }
    public List<CardTranslationItem> Translations { get; set; } = null!;
    public string Term { get; set; } = null!;
    public Guid CreatedBy { get; set; }
    public DateTime NextReviewDate { get; set; }
    public TimeSpan ReviewInterval { get; set; }
    public string Level { get; set; } = null!;
}