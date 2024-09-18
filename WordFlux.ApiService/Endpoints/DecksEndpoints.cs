using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService.Domain;
using WordFlux.ApiService.Persistence;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Endpoints;

public static class DecksEndpoints
{
    public static WebApplication MapDecksEndpoints(this WebApplication app)
    {
        app.MapGet("/decks", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, CancellationToken cancellationToken = default) =>
        {
            var userId = userManager.GetUserId(claimsPrincipal)!;

            return await dbContext.Decks
                .Where(c => c.UserId == userId)
                .Select(d => new DeckDto(d.Id, d.Name, d.Cards.Count, d.CreatedAt, d.Type, d.IsPublic, true))
                .ToListAsync(cancellationToken);
        }).RequireAuthorization();

        app.MapGet("/decks/{deckId:guid}", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, Guid deckId, CancellationToken cancellationToken = default) =>
        {
            var userId = userManager.GetUserId(claimsPrincipal)!;

            return await dbContext.Decks
                .Where(x => x.Id == deckId)
                .Where(x => x.IsPublic || x.UserId == userId)
                .Select(d => new DeckDto(d.Id, d.Name, d.Cards.Count, d.CreatedAt, d.Type, d.IsPublic, d.UserId == userId))
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }).RequireAuthorization();

        
        app.MapPost("/decks", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, CreateDeckRequest request) =>
        {
            var userId = userManager.GetUserId(claimsPrincipal)!;

            var createdDeck = new Deck
            {
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                Id = Guid.NewGuid(),
                Name = request.Name,
                Type = DeckType.Custom
            };

            dbContext.Decks.Add(createdDeck);
            await dbContext.SaveChangesAsync();

            return createdDeck;
        }).RequireAuthorization();
        
        app.MapPost("/decks/{deckId:guid}/duplicate", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, Guid deckId, string duplicateName) =>
        {
            if (string.IsNullOrWhiteSpace(duplicateName))
            {
                return Results.BadRequest();
            }
            
            var userId = userManager.GetUserId(claimsPrincipal)!;
            var deck = await dbContext.Decks.FirstOrDefaultAsync(x => x.Id == deckId);

            if (deck == null)
            {
                return Results.NotFound();
            }

            if (deck.UserId != userId && !deck.IsPublic)
            {
                return Results.Forbid();
            }
    
            var createdDeck = new Deck
            {
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                Id = Guid.NewGuid(),
                Name = duplicateName,
                Type = DeckType.Custom
            };

            dbContext.Decks.Add(createdDeck);
            await dbContext.SaveChangesAsync();
            
            var cards = await dbContext.Cards.AsNoTracking().Where(x => x.DeckId == deckId).ToListAsync();

            var duplicatedCards = cards.Select(x => new Card
            {
                CreatedAt = DateTime.UtcNow,
                DeckId = createdDeck.Id,
                Id = Guid.NewGuid(),
                Term = x.Term,
                Level = x.Level,
                Translations = x.Translations,
                ReviewInterval = TimeSpan.FromMinutes(2),
                CreatedBy = Guid.Parse(userId),
                NextReviewDate = DateTime.UtcNow
            });
            
            dbContext.Cards.AddRange(duplicatedCards);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new CreateDeckResponse(createdDeck.Id, createdDeck.Name));
        }).RequireAuthorization();
        
        app.MapPatch("/decks/{deckId:guid}", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, PatchDeckRequest request, Guid deckId) =>
        {
            var userId = userManager.GetUserId(claimsPrincipal)!;

            var existingDeck = await dbContext.Decks.FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId);

            if (existingDeck == null)
            {
                return Results.NotFound();
            }

            if (request.IsPublic != null)
            {
                existingDeck.IsPublic = request.IsPublic.Value;
            }

            if (request.Name != null)
            {
                existingDeck.Name = request.Name;
            }
            
            await dbContext.SaveChangesAsync();

            return Results.NoContent();

        }).RequireAuthorization();

        
        app.MapDelete("/decks/{deckId:guid}", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, Guid deckId) =>
        {
            var userId = userManager.GetUserId(claimsPrincipal)!;

            var existingDeck = await dbContext.Decks.FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId);

            if (existingDeck == null)
            {
                return Results.NotFound();
            }

            if (existingDeck.Type == DeckType.Default)
            {
                return Results.BadRequest("Could not remove default deck");
            }
            
            dbContext.Decks.Remove(existingDeck);
            await dbContext.SaveChangesAsync();
            
            return Results.NoContent();

        }).RequireAuthorization();

        
        return app;
    }
    
}