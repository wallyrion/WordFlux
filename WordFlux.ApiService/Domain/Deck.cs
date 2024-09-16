
using WordFlux.Contracts;

namespace WordFlux.ApiService.Domain;

public class Deck
{
    public Guid Id { get;  set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<Card> Cards { get; set; } = [];
    public string UserId { get; set; }
    public AppUser User { get; set; }
    public string Name { get; set; } = null!;
    public DeckType Type { get; set; }
}