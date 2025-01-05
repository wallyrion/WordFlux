using System.Security.Claims;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordFlux.Application.Decks.Commands;
using WordFlux.Application.Decks.Queries;
using WordFlux.Application.Events;
using WordFlux.Contracts;
using WordFlux.Domain;
using WordFlux.Domain.Domain;
using WordFlux.Infrastructure.Persistence;
using WordFlux.Translations.Ai;

namespace WordFlux.ApiService.Endpoints;

public static class DecksEndpoints
{
    public static WebApplication MapDecksEndpoints(this WebApplication app)
    {
        app.MapGet("/decks",
                async (ISender sender, CancellationToken cancellationToken = default) => await sender.Send(new GetDecksQuery(), cancellationToken))
            .RequireAuthorization();
        
        app.MapGet("/decks/{deckId:guid}/export",
            async (ApplicationDbContext dbContext, Guid deckId, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager,
                CancellationToken cancellationToken = default) =>
            {
                var userId = userManager.GetUserId(claimsPrincipal)!;

                var deck = await dbContext.Decks
                    .FirstOrDefaultAsync(c => c.UserId == userId && deckId == c.Id, cancellationToken);

                if (deck == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(deck.Export);
            }).RequireAuthorization();

        app.MapGet("/decks/{deckId:guid}", async (Guid deckId, ISender sender, CancellationToken cancellationToken = default) =>
        {
            var deck = await sender.Send(new GetDeckByIdQuery(deckId), cancellationToken);

            if (deck == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(deck);

        });

        
        app.MapPost("/decks", async (CreateDeckRequest request, ISender sender) =>
            {
                var command = new CreateDeckCommand
                {
                    Name = request.Name,
                    Type = DeckType.Custom
                };

                var createdDeck = await sender.Send(command);
                return createdDeck;

            }).RequireAuthorization();
        
        app.MapPost("/decks/parse-export-quizlet",
            async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, ImportDeckRequest request) =>
            {
                var text = Uri.UnescapeDataString(request.Cards);

                var rows = text.Split("%;%");

                var cards = rows.Select(rawRow =>
                {
                    var items = rawRow.Split("%-%");

                    var processingResult = new
                    {
                        Term = items.FirstOrDefault()?.Trim(),
                        Translation = items.Skip(1).FirstOrDefault()?.Trim()
                    };

                    return processingResult;
                }).ToList();

                var verifiedCards = cards.Where(x => !string.IsNullOrEmpty(x.Term) && !string.IsNullOrEmpty(x.Translation)).ToList();
                var cardsWithErrors = cards.Where(x => string.IsNullOrEmpty(x.Term) || string.IsNullOrEmpty(x.Translation)).ToList();

                
            }).RequireAuthorization();


        /*app.MapPost("/test", async (OpenAiGenerator openAiGenerator, ImportDeckRequest request) =>
        {
            
            
            //await openAiGenerator.MapQuizletExportItem(request.Name, request.Cards);
        });*/
        
        app.MapPost("/decks/import",
            async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager, ImportDeckRequest request, OpenAiGenerator openAiGenerator, IPublishEndpoint publishEndpoint) =>
            {
                var text = Uri.UnescapeDataString(request.Cards);

                var rows = text.Split("%;%");

                var cards = rows.Select(rawRow =>
                {
                    var items = rawRow.Split("%-%");

                    if (items.Length == 0)
                    {
                        return null;
                    }

                    if (string.IsNullOrWhiteSpace(items[0].Trim()))
                    {
                        return null;
                    }
                    
                    var processingResult = new DeckExportItem
                    {
                        Term = items[0].Trim(),
                        Translation = items.Skip(1).FirstOrDefault()?.Trim()
                    };

                    return processingResult;
                }).Where(x => x != null).Select(x => x!).ToList();

                var userId = userManager.GetUserId(claimsPrincipal)!;

                var createdDeck = new Deck
                {
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    Id = Guid.NewGuid(),
                    Name = request.Name ?? $"Imported {DateTime.UtcNow.ToLongDateString()}",
                    Type = DeckType.Custom,
                    Export = new DeckExportPayload
                    {
                        Status = DeckExportStatus.Processing,
                        Items = cards,
                        LearnLanguage = request.LearnLanguage,
                        NativeLanguage = request.NativeLanguage,
                    }
                };

                dbContext.Decks.Add(createdDeck);
                await dbContext.SaveChangesAsync();

                var importDeckEvent = new ImportDeckEvent(createdDeck.Id);
                await publishEndpoint.Publish(importDeckEvent);
                /*await openAiGenerator.MapQuizletExportItemList(verifiedCards.Select(x => (x.Term, x.Translation)).ToList()!);

                var verifiedCards = cards.Where(x => !string.IsNullOrEmpty(x.Term) && !string.IsNullOrEmpty(x.Translation)).ToList();
                var cardsWithErrors = cards.Where(x => string.IsNullOrEmpty(x.Term) || string.IsNullOrEmpty(x.Translation)).ToList();

                var duplicatedCards = verifiedCards.Select(x => new Card
                {
                    CreatedAt = DateTime.UtcNow,
                    DeckId = createdDeck.Id,
                    Id = Guid.NewGuid(),
                    Term = x!.Term,
                    Level = "",
                    Translations = [new CardTranslationItem(x.Translation, null, null, 0, null)],
                    ReviewInterval = TimeSpan.FromMinutes(2),
                    CreatedBy = Guid.Parse(userId),
                    Status = CardProcessingStatus.ImportedNotProcessed,
                    NextReviewDate = DateTime.UtcNow,
                });

                dbContext.Cards.AddRange(duplicatedCards);
                await dbContext.SaveChangesAsync();

                var errorRows = cardsWithErrors.Select(c => c.Term ?? c.Translation).Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).ToList();

                return Results.Ok(new ImportedDeckResponse(createdDeck.Id, createdDeck.Name, verifiedCards.Count, errorRows));*/

                return Results.Ok(new ImportedDeckResponse(createdDeck.Id, createdDeck.Export.Items.Count));
            }).RequireAuthorization();

        app.MapPost("/decks/{deckId:guid}/duplicate", async (ApplicationDbContext dbContext, ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager,
            Guid deckId, string duplicateName) =>
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

        app.MapPatch("/decks/{deckId:guid}", async (PatchDeckRequest request, Guid deckId, ISender sender, CancellationToken cancellationToken) =>
        {
            var patchCommand = new PatchDeckCommand
            {
                DeckId = deckId,
                Name = request.Name,
                IsPublic = request.IsPublic
            };
            
            await sender.Send(patchCommand, cancellationToken);

            return Results.NoContent();
        }).RequireAuthorization();

        app.MapDelete("/decks/{deckId:guid}",
            async (Guid deckId, ISender sender) =>
            {
                var command = new DeleteDeckCommand
                {
                    DeckId = deckId
                };
                await sender.Send(command);
                
                return Results.NoContent();
            }).RequireAuthorization();

        return app;
    }
}