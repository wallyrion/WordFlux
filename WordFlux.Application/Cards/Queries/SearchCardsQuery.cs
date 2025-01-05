using MediatR;
using Microsoft.EntityFrameworkCore;
using WordFlux.Contracts;
using WordFlux.Domain;

namespace WordFlux.Application.Cards.Queries;

public class SearchCardsQuery : IRequest<SearchCardResponse>
{
    public required string Keyword { get; init; }
    public int Limit { get; init; } = 100;
    public int Offset { get; init; } = 0;
}

public class SearchCardsQueryHandler(IDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<SearchCardsQuery, SearchCardResponse>
{
    public async Task<SearchCardResponse> Handle(SearchCardsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.GetUserId();

        var query = dbContext.Cards
            .AsNoTracking()
            .Where(x => x.CreatedBy.ToString() == currentUserId)
            .Where(x => x.Term == request.Keyword);
        
        var count = await query.CountAsync(cancellationToken);

        if (count == 0)
        {
            return new SearchCardResponse([], 0);
        }
        
        var cards = query
            .Select(CardMapper.ToCardDto())
            .Take(request.Limit)
            .Skip(request.Offset)
            .ToList();

        return new SearchCardResponse(cards, count);
    }
}