using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService.Domain;
using WordFlux.ApiService.Persistence.DbConfigurations;

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
