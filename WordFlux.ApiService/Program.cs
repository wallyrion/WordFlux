using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.AddNpgsqlDbContext<ApplicationDbContext>("postgresdb");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (ApplicationDbContext dbContext, ILogger<Program> logger) =>
{
    // get connection string
    var connectionString = dbContext.Database.GetConnectionString();
    logger.LogInformation("Connection string: {connectionString}", connectionString);
    
    await dbContext.Database.EnsureCreatedAsync();
    
    var item = new WeatherForecast
    {
        Id = Guid.NewGuid(),
        TemperatureC = Random.Shared.Next(-20, 55),
        CreatedAt = DateTime.UtcNow
    };

    dbContext.Forecasts.Add(item);
    await dbContext.SaveChangesAsync();

    return await dbContext.Forecasts.ToListAsync();

});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecastDto(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
