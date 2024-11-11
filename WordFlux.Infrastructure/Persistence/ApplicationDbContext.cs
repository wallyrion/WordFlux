using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WordFlux.Domain;
using WordFlux.Domain.Domain;
using WordFlux.Infrastructure.Persistence.DbConfigurations;

namespace WordFlux.Infrastructure.Persistence;

// ensure db created
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser>(options), IDbContext
{
    public DbSet<Card> Cards { get; set; }
    public DbSet<Deck> Decks { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CardConfiguration());
        modelBuilder.ApplyConfiguration(new DeckConfiguration());
    }
}
