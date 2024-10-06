using WordFlux.ApiService.ViewModels;
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
    public string? NativeLanguage { get; set; }
    public string? LearnLanguage { get; set; } 
    
    public string? SourceLanguage { get; set; }
    public string? TargetLanguage { get; set; }
    
    public Deck Deck { get; set; } = null!;
    public Guid DeckId { get; set; }
    public string? ImageUrl { get; set; }
    public CardProcessingStatus Status { get; set; }
}