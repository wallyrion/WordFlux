using MediatR;
using Microsoft.EntityFrameworkCore;
using WordFlux.Contracts;
using WordFlux.Domain;

namespace WordFlux.Application.Decks.Queries;

public class GetDecksQuery : IRequest<IReadOnlyList<DeckDto>>;

public class GetDecksQueryHandler(ICurrentUser currentUser, IDbContext dbContext) : IRequestHandler<GetDecksQuery, IReadOnlyList<DeckDto>>
{
    public async Task<IReadOnlyList<DeckDto>> Handle(GetDecksQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.GetUserId();

        var result = await dbContext.Decks
            .Where(c => c.UserId == currentUserId)
            .OrderBy(x => x.Type)
            .ThenBy(x => x.CreatedAt)
            .Select(Mapper.ToDto(currentUserId: currentUserId))
            .ToListAsync(cancellationToken: cancellationToken);

        return result;
    }
}

