using FluentValidation;
using MediatR;
using WordFlux.Contracts;
using WordFlux.Domain;
using WordFlux.Domain.Domain;

namespace WordFlux.Application.Decks.Commands;

public class CreateDeckCommand : IRequest<Deck>
{
    public required string Name { get; init; }
    public required DeckType Type { get; init; }
}

public class CreateDeckCommandValidation : AbstractValidator<CreateDeckCommand>
{
    public CreateDeckCommandValidation()
    {
        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);
    }
}

public class CreateDeckCommandHandler(ICurrentUser currentUser, IDbContext dbContext) : IRequestHandler<CreateDeckCommand, Deck>
{
    public async Task<Deck> Handle(CreateDeckCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.GetUserId();
        
        var createdDeck = new Deck
        {
            CreatedAt = DateTime.UtcNow,
            UserId = currentUserId,
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = DeckType.Custom
        };

        dbContext.Decks.Add(createdDeck);
        await dbContext.SaveChangesAsync(cancellationToken);

        return createdDeck;
    }
}

