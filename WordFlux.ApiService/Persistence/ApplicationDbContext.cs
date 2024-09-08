using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService.Domain;
using WordFlux.ApiService.Persistence.DbConfigurations;

namespace WordFlux.ApiService.Persistence;

// ensure db created
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :  IdentityDbContext<AppUser>(options)
{
    public DbSet<Card> Cards { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CardConfiguration());
    }
}
