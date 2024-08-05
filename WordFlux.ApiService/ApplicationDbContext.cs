using Microsoft.EntityFrameworkCore;

namespace WordFlux.ApiService;

// ensure db created
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<WeatherForecast> Forecasts { get; set; }
}


public class WeatherForecast
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TemperatureC { get; set; }
}