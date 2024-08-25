using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using WordFlux.ApiService.Domain;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Endpoints;

public static class CardsEndpoints
{
    public static WebApplication MapCardsEndpoints(this WebApplication app)
    {
        app.MapGet("/cards", async (ApplicationDbContext dbContext, Guid userId) =>
        {
            return await dbContext.Cards.Where(c => c.CreatedBy == userId).ToListAsync();
        });

        app.MapDelete("/cards/{cardId:guid}", async (ApplicationDbContext dbContext, Guid userId, Guid cardId) =>
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
        
        app.MapGet("/cards/next", async (ApplicationDbContext dbContext, Guid userId, int? skip = 0) =>
        {
            skip ??= 0;

            return await dbContext.Cards
                .Where(c => c.CreatedBy == userId && c.NextReviewDate < DateTime.UtcNow)
                .OrderBy(x => x.NextReviewDate)
                .Skip(skip.Value).FirstOrDefaultAsync();
        });

        
        app.MapGet("/cards/next/time", async (ApplicationDbContext dbContext, Guid userId) =>
        {
            var nextCard = await dbContext.Cards
                .Where(c => c.CreatedBy == userId)
                .OrderBy(x => x.NextReviewDate)
                .Select(x => new {x.NextReviewDate}).FirstOrDefaultAsync();
    
            if (nextCard == null)
            {
                return Results.Ok(new NextReviewCardTimeResponse (TimeSpan.MaxValue));
            }
    
            return Results.Ok(new NextReviewCardTimeResponse (nextCard.NextReviewDate - DateTime.UtcNow));
        });
        
        app.MapPost("/cards/{cardId:guid}/approve", async (ApplicationDbContext dbContext, Guid cardId, Guid userId) =>
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
        
        
        app.MapPost("/cards/{cardId:guid}/reject", async (ApplicationDbContext dbContext, Guid cardId, Guid userId) =>
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
        
        
        app.MapPost("/cards", async (ApplicationDbContext dbContext, CardRequest request, Guid userId) =>
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
        
        return app;
    }
    
}