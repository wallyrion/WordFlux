using Microsoft.EntityFrameworkCore;
using WordFlux.Domain.Domain;

namespace WordFlux.Domain;

public interface IDbContext : IAsyncDisposable, IDisposable
{
    public DbSet<Card> Cards { get; set; }
    public DbSet<Deck> Decks { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}