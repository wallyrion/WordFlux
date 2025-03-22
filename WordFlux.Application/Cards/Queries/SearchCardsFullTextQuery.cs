using MediatR;
using Microsoft.EntityFrameworkCore;
using WordFlux.Application.Common.Abstractions;
using WordFlux.Contracts;
using WordFlux.Domain;
using WordFlux.Domain.Domain;

namespace WordFlux.Application.Cards.Queries;

public class SearchCardsFullTextCardsQuery : IRequest<SearchCardResponse>
{
    public required string Keyword { get; init; }
    public int Limit { get; init; } = 100;
    public int Offset { get; init; } = 0;
}

public class SearchCardsFullTextCardsQueryHandler(IDbContext dbContext, ICurrentUser currentUser, ISearchService searchService) : IRequestHandler<SearchCardsFullTextCardsQuery, SearchCardResponse>
{
    public async Task<SearchCardResponse> Handle(SearchCardsFullTextCardsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.GetUserId();

        var foundCardIds = await searchService.SearchCardsAsync(currentUserId, request.Keyword, cancellationToken);
        
        var cards = await dbContext.Cards
            .Where(c => foundCardIds.Select(f => f.cardId).Contains(c.Id))
            .Include(c => c.Deck)
            .Include(c => c.Translations)
            .Include(c => c.ExampleTasks)
            .ToListAsync(cancellationToken);

        var result = cards
            .Join(foundCardIds,
                card => card.Id,
                found => found.cardId,
                (card, found) => new { Card = card, found.Score })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Card.ToCardDto())
            .ToList();

        var searchResult = new SearchCardResponse(result, result.Count);
        
        return searchResult;
    }
}