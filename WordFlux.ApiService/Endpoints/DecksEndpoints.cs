using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
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

public static class DecksEndpoints
{
    public static WebApplication MapDecksEndpoints(this WebApplication app)
    {
        app.MapGet("/decks", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, CancellationToken cancellationToken = default) =>
        {
            var userId = userManager.GetUserId(claimsPrincipal)!;

            return await dbContext.Decks
                .Where(c => c.UserId == userId)
                .Select(d => new DeckDto(d.Id, d.Name, d.Cards.Count, d.CreatedAt, d.Type))
                .ToListAsync(cancellationToken);
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
        
        app.MapPut("/decks/{deckId:guid}", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, CreateDeckRequest request, Guid deckId) =>
        {
            var userId = userManager.GetUserId(claimsPrincipal)!;

            var existingDeck = await dbContext.Decks.FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId);

            if (existingDeck == null)
            {
                return Results.NotFound();
            }

            existingDeck.Name = request.Name;
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