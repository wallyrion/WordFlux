using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService.DbConfigurations;

namespace WordFlux.ApiService;

// ensure db created
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Card> Cards { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CardConfiguration());
    }
}


public class Card
{
    public Guid Id { get;  set; }
    public DateTime CreatedAt { get; set; }
    public List<CardTranslationItem> Translations { get; set; }
    public string Term { get; set; }
}

/*public interface IEntity
{
    Guid Id { get; init; }
    DateTime CreatedAt { get; init; }
}*/