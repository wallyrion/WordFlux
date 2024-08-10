using Microsoft.EntityFrameworkCore;
using TwitPoster.Web.WebHostServices;
using WordFlux.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSwagger();
builder.Services.AddEndpointsApiExplorer();
builder.AddNpgsqlDbContext<ApplicationDbContext>("postgresdb");
builder.Services.AddHostedService<MigrationHostedService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/cards", async (ApplicationDbContext dbContext, ILogger<Program> logger) =>
{
    await dbContext.Database.EnsureCreatedAsync();
    return await dbContext.Cards.ToListAsync();
});
app.MapPost("/cards", async (ApplicationDbContext dbContext, ILogger<Program> logger, CardRequest request) =>
{
    var card = new Card
    {
        CreatedAt = DateTime.UtcNow,
        Id= Guid.NewGuid(),
        Term = request.Term,
        Translation = request.Translation,
        Example = request.Example
    };
    dbContext.Cards.Add(card);
    await dbContext.SaveChangesAsync();
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecastDto(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record CardRequest(string Term, string Translation, string Example);