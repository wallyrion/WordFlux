using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordFlux.Domain;
using WordFlux.Domain.Exceptions;

namespace WordFlux.Application.Decks.Commands;

public class PatchDeckCommand : IRequest
{
    public required Guid DeckId { get; init; }
    public string? Name { get; init; }
    public bool? IsPublic { get; init; }
}

public class PatchDeckCommandValidation : AbstractValidator<PatchDeckCommand>
{
    public PatchDeckCommandValidation()
    {
        RuleFor(x => x.DeckId)
            .NotEmpty();
    }
}

public class PatchDeckCommandHandler(ICurrentUser currentUser, IDbContext dbContext) : IRequestHandler<PatchDeckCommand>
{
    public async Task Handle(PatchDeckCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId()!;

        var existingDeck = await dbContext.Decks.FirstOrDefaultAsync(d => d.Id == request.DeckId && d.UserId == userId, cancellationToken: cancellationToken);

        if (existingDeck == null)
        {
            throw new DomainValidationException("Could not find deck with the specified id");
        }

        if (request.IsPublic != null)
        {
            existingDeck.IsPublic = request.IsPublic.Value;
        }

        if (request.Name != null)
        {
            existingDeck.Name = request.Name;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

