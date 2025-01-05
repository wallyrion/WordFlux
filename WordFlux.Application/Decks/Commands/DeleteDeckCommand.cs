using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordFlux.Contracts;
using WordFlux.Domain;
using WordFlux.Domain.Exceptions;

namespace WordFlux.Application.Decks.Commands;

public class DeleteDeckCommand : IRequest
{
    public required Guid DeckId { get; init; }
}

public class DeleteDeckCommandValidator : AbstractValidator<DeleteDeckCommand>
{
    public DeleteDeckCommandValidator()
    {
        RuleFor(x => x.DeckId)
            .NotEmpty();
    }
}


public class DeleteDeckCommandHandler(ICurrentUser currentUser, IDbContext dbContext) : IRequestHandler<DeleteDeckCommand>
{
    public async Task Handle(DeleteDeckCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId()!;

        var existingDeck = await dbContext.Decks.FirstOrDefaultAsync(d => d.Id == request.DeckId && d.UserId == userId, cancellationToken: cancellationToken);

        if (existingDeck == null)
        {
            throw new DomainValidationException("Could not find deck with the specified id");
        }

        if (existingDeck.Type == DeckType.Default)
        {
            throw new DomainValidationException("Could not remove default deck");
        }

        dbContext.Decks.Remove(existingDeck);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

