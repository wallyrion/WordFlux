using MediatR;
using Microsoft.EntityFrameworkCore;
using WordFlux.Contracts;
using WordFlux.Domain;

namespace WordFlux.Application.Decks.Queries;

public record GetDeckByIdQuery(Guid DeckId) : IRequest<DeckDto?>;

public class GetDeckByIdQueryHandler(ICurrentUser currentUser, IDbContext dbContext) : IRequestHandler<GetDeckByIdQuery, DeckDto?>
{
    public async Task<DeckDto?> Handle(GetDeckByIdQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.GetUserId();
        
        return await dbContext.Decks
            .Where(x => x.Id == request.DeckId)
            .Where(x => x.IsPublic || x.UserId == currentUserId)
            .Select(Mapper.ToDto(currentUserId: currentUserId))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}

