using System.Linq.Expressions;
using WordFlux.Contracts;
using WordFlux.Domain.Domain;

namespace WordFlux.Application.Decks;

public static partial class Mapper
{
    public static Expression<Func<Deck, DeckDto>> ToDto(string currentUserId)
    {
        return d => new DeckDto(d.Id, d.Name, d.Cards.Count, d.CreatedAt, d.Type, d.IsPublic, d.UserId == currentUserId);
    }
}