using MediatR;
using Microsoft.EntityFrameworkCore;
using WordFlux.Application.Common.Abstractions;
using WordFlux.Contracts;
using WordFlux.Domain;

namespace WordFlux.Application.Cards.Queries;

public class SearchCardsFullTextCardsQuery : IRequest<List<TestCard>>
{
    public required string Keyword { get; init; }
    public int Limit { get; init; } = 100;
    public int Offset { get; init; } = 0;
}

public class SearchCardsFullTextCardsQueryHandler(IDbContext dbContext, ICurrentUser currentUser, ISearchService searchService) : IRequestHandler<SearchCardsFullTextCardsQuery, List<TestCard>>
{
    public async Task<List<TestCard>> Handle(SearchCardsFullTextCardsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.GetUserId();

        var res = await searchService.SearchAsync(request.Keyword, cancellationToken);

        return res;
    }
}