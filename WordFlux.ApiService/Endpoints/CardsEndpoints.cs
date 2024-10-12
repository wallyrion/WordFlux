using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using WordFlux.ApiService.Domain;
using WordFlux.ApiService.Jobs;
using WordFlux.ApiService.Mappers;
using WordFlux.ApiService.Persistence;
using WordFlux.ApiService.ViewModels;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Endpoints;

public static class CardsEndpoints
{
    public static WebApplication MapCardsEndpoints(this WebApplication app)
    {
        app.MapGet("/cards/{cardId:guid}",
            async (ApplicationDbContext dbContext, Guid cardId, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager,
                CancellationToken cancellationToken = default) =>
            {
                var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

                var card = await dbContext.Cards
                    .Where(c => c.CreatedBy == userId && c.Id == cardId)
                    .AsNoTracking()
                    .Select(CardMapper.ToCardDto())
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                return card;
            }).RequireAuthorization();

        app.MapGet("/cards",
            async (ApplicationDbContext dbContext, Guid? deckId, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager,
                CancellationToken cancellationToken = default) =>
            {
                var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

                var query = dbContext.Cards
                    .OrderByDescending(x => x.CreatedAt)
                    .AsQueryable();

                if (deckId != null)
                {
                    query = query.Where(c => c.DeckId == deckId);

                    query = query.Where(c => c.CreatedBy == userId || c.Deck.IsPublic);
                }
                else
                {
                    query = query.Where(c => c.CreatedBy == userId);
                }

                var result = await query
                    .AsNoTracking()
                    .Select(CardMapper.ToCardDto())
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

        app.MapGet("/cards/next", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager,
            ParsableQueryList? deckIds = null, int? skip = 0) =>
        {
            var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

            skip ??= 0;

            var query = dbContext.Cards
                .AsNoTracking()
                .Where(x => x.CreatedBy == userId && x.NextReviewDate < DateTime.UtcNow);

            if (deckIds?.Ids is { Count: > 0 })
            {
                query = query.Where(x => deckIds.Ids.Contains(x.DeckId));
            }

            return await query
                .OrderBy(x => x.NextReviewDate)
                .Skip(skip.Value)
                .Select(CardMapper.ToCardDto())
                .FirstOrDefaultAsync();
        }).RequireAuthorization();

        app.MapGet("/cards/next/time",
            async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, ParsableQueryList? deckIds = null) =>
            {
                var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

                var query = dbContext.Cards
                    .Where(c => c.CreatedBy == userId);

                if (deckIds?.Ids is { Count: > 0 })
                {
                    query = query.Where(x => deckIds.Ids.Contains(x.DeckId));
                }

                var nextCard = await query
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
                CardMessagePublisher messagePublisher,
                UserManager<AppUser> userManager) =>
            {
                var userIdStr = userManager.GetUserId(claimsPrincipal);
                var userId = Guid.Parse(userIdStr!);

                var defaultDeck = await dbContext.Decks.FirstOrDefaultAsync(f => f.Type == DeckType.Default && f.UserId == userIdStr);

                if (defaultDeck == null)
                {
                    defaultDeck = new Deck
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
                    Translations = request.Translations.Select(x => new CardTranslationItem(x.Term, x.ExampleTranslated, x.ExampleOriginal, x.Popularity, x.Level))
                        .ToList(),
                    CreatedBy = userId,
                    NextReviewDate = DateTime.UtcNow,
                    ReviewInterval = TimeSpan.FromMinutes(2),
                    Level = request.Level,
                    DeckId = request.DeckId == default ? defaultDeck.Id : request.DeckId,
                    ImageUrl = request.ImageUrl,
                    SourceLanguage = request.SourceLang,
                    LearnLanguage = request.LearnLang,
                    TargetLanguage = request.TargetLang,
                    NativeLanguage = request.NativeLang,
                    Status = request.SourceLang == null ? CardProcessingStatus.Unprocessed : CardProcessingStatus.LanguageDetected
                };

                dbContext.Cards.Add(card);
                await dbContext.SaveChangesAsync();

                if (card.SourceLanguage == null)
                {
                    await messagePublisher.PublishNewCardForLanguageDetection(card.Id);
                }
                else
                {
                    await messagePublisher.PublishNewCardForTasksCreating(card.Id);
                }

                logger.LogInformation("Saving card for term = {Term}", request.Term);

                return await dbContext.Cards.AsNoTracking().Where(x => x.Id == card.Id).Select(CardMapper.ToCardDto()).FirstOrDefaultAsync();
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
            existingCard.ImageUrl = request.ImageUrl;

            await dbContext.SaveChangesAsync();

            return Results.Ok();
        }).RequireAuthorization();
        
        app.MapPatch("/cards/{cardId:guid}", async (ApplicationDbContext dbContext, PatchCardRequest request, Guid cardId,
            ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
        {
            var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

            var existingCard = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId && userId == x.CreatedBy);

            if (existingCard == null)
            {
                return Results.NotFound();
            }
            
            existingCard.UpdateProperty(x => x.Term, request.Term);
            existingCard.UpdateProperty(x => x.ImageUrl, request.ImageUrl);
            existingCard.UpdateProperty(x => x.DeckId, request.DeckId);
            existingCard.UpdateProperty(x => x.Translations, request.Translations);

            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
        
        app.MapDelete("/cards/{cardId:guid}/image", async (ApplicationDbContext dbContext, Guid cardId,
            ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
        {
            var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

            var existingCard = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId && userId == x.CreatedBy);

            if (existingCard == null)
            {
                return Results.NotFound();
            }
            
            existingCard.ImageUrl = null;
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapPut("/cards/{cardId:guid}/challenges/regenerate", async (ApplicationDbContext dbContext, ILogger<Program> logger, Guid cardId,
            ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, [FromServices] CardMessagePublisher publisher) =>
        {
            var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

            var existingCard = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId && userId == x.CreatedBy);

            if (existingCard == null)
            {
                return Results.NotFound();
            }

            await publisher.PublishNewCardForTasksCreating(cardId);

            return Results.Ok();
        }).RequireAuthorization();

        return app;
    }
    
    static void UpdateProperty<TSource, TResult>(this TSource obj, Expression<Func<TSource, TResult>> keySelector, TResult? newValue) where TSource : class
    {
        if (newValue == null)
        {
            return;
        }

        var memberExpression = keySelector.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new ArgumentException("The expression should represent a property access.", nameof(keySelector));
        }

        // Get the property info from the member expression
        var propertyInfo = (System.Reflection.PropertyInfo)memberExpression.Member;

        // Set the value of the property
        propertyInfo.SetValue(obj, newValue, null);
    }
}

public class ParsableQueryList : IParsable<ParsableQueryList>
{
    public IReadOnlyCollection<Guid> Ids { get; private set; } = [];

    public static ParsableQueryList Parse(string s, IFormatProvider? provider)
    {
        var entries = s.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var guids = entries.Select(Guid.Parse);

        return new ParsableQueryList
        {
            Ids = guids.ToList()
        };
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ParsableQueryList result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = new ParsableQueryList();

            return false;
        }

        var entries = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var guids = entries.Select(Guid.Parse);

        result = new ParsableQueryList
        {
            Ids = guids.ToList()
        };

        return true;
    }
    
}

