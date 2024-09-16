using WordFlux.Contracts;

namespace WordFLux.ClientApp.Models;

public record DeckDto(Guid Id, DateTimeOffset CreatedAt, string UserId, string Name, DeckType Type);