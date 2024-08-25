using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using TwitPoster.Web.WebHostServices;
using WordFlux.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.Services.AddOutputCache();

if (builder.Configuration["UseAzureKeyVault"] == "true")
{
    Console.WriteLine("Using Azure Key Vault");
    builder.Configuration.AddAzureKeyVaultSecrets("secrets");
}

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSwagger();
builder.Services.AddEndpointsApiExplorer();
builder.AddNpgsqlDbContext<ApplicationDbContext>("postgresdb");
builder.Services.AddHostedService<MigrationHostedService>();

builder.Services.AddOpenAi(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseOutputCache();
app.UseExceptionHandler();


app.MapGet("/audio/link", async (string term) =>
{
    var url = "https://apiservice.jollycliff-5a69ab58.westeurope.azurecontainerapps.io";

    return new { Link = $"{url}/audio?term={Uri.EscapeDataString(term)}" };
}).CacheOutput();

#pragma warning disable SKEXP0001
app.MapGet("/audio", async ([FromServices] Kernel kernel, string term) =>
{
#pragma warning disable SKEXP0001
    var service = kernel.GetRequiredService<ITextToAudioService>();
#pragma warning restore SKEXP0001

    var res = await service.GetAudioContentsAsync(term);

    var first = res[0];

    return Results.File(first.Data!.Value.ToArray(), "audio/mp3");
    //return Results.File(file, "audio/mp3");
}).WithName("audio").CacheOutput();

app.MapGet("/cards",
    async (ApplicationDbContext dbContext, ILogger<Program> logger, Guid userId) =>
    {
        return await dbContext.Cards.Where(c => c.CreatedBy == userId).ToListAsync();
    });

app.MapDelete("/cards/{cardId:guid}", async (ApplicationDbContext dbContext, ILogger<Program> logger, Guid userId, Guid cardId) =>
{
    var card = await dbContext.Cards.Where(c => c.CreatedBy == userId && c.Id == cardId).FirstOrDefaultAsync();

    if (card == null)
    {
        return Results.NotFound();
    }

    dbContext.Cards.Remove(card);
    await dbContext.SaveChangesAsync();

    return Results.Ok();
});

app.MapGet("/cards/next", async (ApplicationDbContext dbContext, ILogger<Program> logger, Guid userId, int? skip = 0) =>
{
    skip ??= 0;

    return await dbContext.Cards
        .Where(c => c.CreatedBy == userId && c.NextReviewDate < DateTime.UtcNow)
        .OrderBy(x => x.NextReviewDate)
        .Skip(skip.Value).FirstOrDefaultAsync();
});



app.MapGet("/cards/next/time", async (ApplicationDbContext dbContext, ILogger<Program> logger, Guid userId) =>
{
    var nextCard = await dbContext.Cards
        .Where(c => c.CreatedBy == userId)
        .OrderBy(x => x.NextReviewDate)
        .Select(x => new {x.NextReviewDate}).FirstOrDefaultAsync();
    
    if (nextCard == null)
    {
        return Results.Ok(new { TimeToNextReview = TimeSpan.MaxValue });
    }
    
    logger.LogInformation("Time to next review: {TimeToNextReview}", nextCard.NextReviewDate - DateTime.UtcNow);
    
    return Results.Ok(new { TimeToNextReview = nextCard.NextReviewDate - DateTime.UtcNow });
});
app.MapPost("/cards/{cardId:guid}/approve", async (ApplicationDbContext dbContext, ILogger<Program> logger, Guid cardId, Guid userId) =>
{
    var existingCard = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId && userId == x.CreatedBy);

    if (existingCard == null)
    {
        return Results.NotFound();
    }

    var reviewInterval = existingCard.ReviewInterval * 2;
    var nextReviewDate = DateTime.UtcNow + reviewInterval + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 10000));

    existingCard.NextReviewDate = nextReviewDate;
    existingCard.ReviewInterval = reviewInterval;
    await dbContext.SaveChangesAsync();

    return Results.Ok();
});

app.MapPost("/cards/{cardId:guid}/reject", async (ApplicationDbContext dbContext, ILogger<Program> logger, Guid cardId, Guid userId) =>
{
    var existingCard = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId && userId == x.CreatedBy);

    if (existingCard == null)
    {
        return Results.NotFound();
    }

    var nextReviewDate = DateTime.UtcNow + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 10000));

    existingCard.NextReviewDate = nextReviewDate;
    await dbContext.SaveChangesAsync();

    return Results.Ok();
});

app.MapPost("/cards", async (ApplicationDbContext dbContext, ILogger<Program> logger, CardRequest request, Guid userId) =>
{
    var card = new Card
    {
        CreatedAt = DateTime.UtcNow,
        Id = Guid.NewGuid(),
        Term = request.Term,
        Translations = request.Translations,
        CreatedBy = userId,
        NextReviewDate = DateTime.MinValue,
        ReviewInterval = TimeSpan.FromMinutes(2),
        Level = request.Level
    };

    dbContext.Cards.Add(card);
    await dbContext.SaveChangesAsync();
});

app.MapPut("/cards/{cardId:guid}", async (ApplicationDbContext dbContext, ILogger<Program> logger, CardRequest request, Guid userId, Guid cardId) =>
{
    var existingCard = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId && userId == x.CreatedBy);

    if (existingCard == null)
    {
        return Results.NotFound();
    }

    existingCard.Term = request.Term;
    existingCard.Level = request.Level;
    existingCard.Translations = request.Translations;

    await dbContext.SaveChangesAsync();

    return Results.Ok();
});

#pragma warning disable SKEXP0010

app.MapGet("/term/level", async (ApplicationDbContext dbContext, ILogger<Program> logger, string term, OpenAiGenerator translation) =>
{
    var respose = await translation.GetLevel(term);

    return new { Level = respose };
});

app.MapGet("/term", async (ApplicationDbContext dbContext, ILogger<Program> logger, string term, OpenAiGenerator translation) =>
{
    var response = await translation.GetTranslations(term);

    return response;
});

app.MapGet("/translations", async (ApplicationDbContext dbContext, ILogger<Program> logger, string term, OpenAiGenerator translation) =>
{
    var respose = await translation.GetTranslations(term);

    return respose;
});

app.MapGet("/motivation", async (ApplicationDbContext dbContext, ILogger<Program> logger, OpenAiGenerator translation) =>
{
    var response = await translation.GetMotivationalPhrase();

    return new { Phrase = response };
}).CacheOutput();

app.MapPost("/translations/examples", async (ApplicationDbContext dbContext, ILogger<Program> logger, GetTranslationExamples request, OpenAiGenerator ai) =>
{
    if (string.IsNullOrEmpty(request.SourceLanguage) || string.IsNullOrEmpty(request.DestinationLanguage))
    {
        (string srcLang, string destLang)? detectedLanguages = await ai.DetectLanguage(request.Term, request.Translations.First());
        
        if (detectedLanguages == null)
        {
            return Results.BadRequest("Could not detect languages");
        }
        
        request = request with {SourceLanguage = detectedLanguages.Value.srcLang, DestinationLanguage = detectedLanguages.Value.destLang};
    }
    
    var response = await ai.GetExamples(request.Term, request.Translations, request.SourceLanguage, request.DestinationLanguage);

    return Results.Ok(response);
});

#pragma warning restore SKEXP0010

app.MapDefaultEndpoints();

app.Run();

public record GetTranslationExamples(string Term, List<string> Translations, string SourceLanguage, string DestinationLanguage);