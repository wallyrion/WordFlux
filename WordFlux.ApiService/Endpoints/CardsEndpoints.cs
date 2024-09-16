using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using WordFlux.ApiService.Domain;
using WordFlux.ApiService.Persistence;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Endpoints;

public static class CardsEndpoints
{
    public static WebApplication MapCardsEndpoints(this WebApplication app)
    {
        app.MapGet("/cards",
            async (ApplicationDbContext dbContext, Guid? deckId, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager,
                CancellationToken cancellationToken = default) =>
            {
                var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

                var query = dbContext.Cards.Where(c => c.CreatedBy == userId);

                if (deckId != null)
                {
                    query = query.Where(c => c.DeckId == deckId);
                }

                var result = await query
                    .AsNoTracking()
                    .Select(x => new CardDto(x.Id, x.CreatedAt, x.Term, x.Level, x.Translations, x.ReviewInterval, x.Deck.Name))
                    .ToListAsync(cancellationToken: cancellationToken);

                return result;
            }).RequireAuthorization();

        app.MapDelete("/cards/{cardId:guid}",
            async (ApplicationDbContext dbContext, Guid cardId, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
            {
                var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

                var card = await dbContext.Cards.Where(c => c.CreatedBy == userId && c.Id == cardId).FirstOrDefaultAsync();

                if (card == null)
                {
                    return Results.NotFound();
                }

                dbContext.Cards.Remove(card);
                await dbContext.SaveChangesAsync();

                return Results.Ok();
            }).RequireAuthorization();

        app.MapGet("/cards/next", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, int? skip = 0) =>
        {
            var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

            skip ??= 0;

            return await dbContext.Cards
                .AsNoTracking()
                .Where(c => c.CreatedBy == userId && c.NextReviewDate < DateTime.UtcNow)
                .OrderBy(x => x.NextReviewDate)
                .Skip(skip.Value)
                .Select(x => new CardDto(x.Id, x.CreatedAt, x.Term, x.Level, x.Translations, x.ReviewInterval, x.Deck.Name))
                .FirstOrDefaultAsync();
        }).RequireAuthorization();

        app.MapGet("/cards/next/time", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
        {
            var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

            var nextCard = await dbContext.Cards
                .Where(c => c.CreatedBy == userId)
                .OrderBy(x => x.NextReviewDate)
                .Select(x => new { x.NextReviewDate }).FirstOrDefaultAsync();

            if (nextCard == null)
            {
                return Results.Ok(new NextReviewCardTimeResponse(null));
            }

            return Results.Ok(new NextReviewCardTimeResponse(nextCard.NextReviewDate - DateTime.UtcNow));
        }).RequireAuthorization();

        app.MapPost("/cards/{cardId:guid}/approve",
            async (ApplicationDbContext dbContext, Guid cardId, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
            {
                var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

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
            }).RequireAuthorization();

        app.MapPost("/cards/{cardId:guid}/reject",
            async (ApplicationDbContext dbContext, Guid cardId, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
            {
                var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

                var existingCard = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId && userId == x.CreatedBy);

                if (existingCard == null)
                {
                    return Results.NotFound();
                }

                var reviewInternal = existingCard.ReviewInterval / 2;
                var nextReviewDate = DateTime.UtcNow + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 10000)) + reviewInternal;

                existingCard.NextReviewDate = nextReviewDate;
                existingCard.ReviewInterval = reviewInternal;
                await dbContext.SaveChangesAsync();

                return Results.Ok();
            }).RequireAuthorization();

        app.MapPost("/cards",
            async (ILogger<Program> logger, ApplicationDbContext dbContext, CardRequest request, ClaimsPrincipal claimsPrincipal,
                UserManager<AppUser> userManager) =>
            {
                var userIdStr = userManager.GetUserId(claimsPrincipal);
                var userId = Guid.Parse(userIdStr!);

                var defaultDeck = await dbContext.Decks.FirstOrDefaultAsync(f => f.Type == DeckType.Default && f.UserId == userIdStr);

                if (defaultDeck == null)
                {
                    defaultDeck = new Deck()
                    {
                        CreatedAt = DateTime.UtcNow,
                        UserId = userIdStr!,
                        Id = Guid.NewGuid(),
                        Type = DeckType.Default,
                        Name = "Default",
                    };

                    dbContext.Decks.Add(defaultDeck);
                    await dbContext.SaveChangesAsync();
                }

                var card = new Card
                {
                    CreatedAt = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    Term = request.Term,
                    Translations = request.Translations,
                    CreatedBy = userId,
                    NextReviewDate = DateTime.UtcNow,
                    ReviewInterval = TimeSpan.FromMinutes(2),
                    Level = request.Level,
                    DeckId = request.DeckId == default ? defaultDeck.Id : request.DeckId,
                };

                dbContext.Cards.Add(card);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("Saving card for term = {Term}", request.Term);
            }).RequireAuthorization();

        app.MapPut("/cards/{cardId:guid}", async (ApplicationDbContext dbContext, ILogger<Program> logger, CardRequest request, Guid cardId,
            ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
        {
            var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

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
        }).RequireAuthorization();

        return app;
    }
}