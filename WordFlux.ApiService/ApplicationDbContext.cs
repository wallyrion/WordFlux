using Microsoft.EntityFrameworkCore;

namespace WordFlux.ApiService;

// ensure db created
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Card> Cards { get; set; }
}


public class Card
{
    public Guid Id { get;  set; }
    public DateTime CreatedAt { get; set; }
    public string Term { get; set; }
    public string Translation { get; set;  } 
    public string Example { get; set; }
}

/*public interface IEntity
{
    Guid Id { get; init; }
    DateTime CreatedAt { get; init; }
}*/