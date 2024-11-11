using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordFlux.Application;
using WordFlux.Application.Events;
using WordFlux.Application.Jobs;
using WordFlux.Contracts;
using WordFlux.Domain.Domain;
using WordFlux.Infrastructure.Persistence;
using WordFlux.Translations.Ai;

namespace WordFlux.Infrastructure.Messaging.Consumers;

public class ImportDeckEventConsumer(
    CardMessagePublisher cardMessagePublisher,
    OpenAiGenerator openAiGenerator,
    ApplicationDbContext dbContext,
    ILogger<ImportDeckEventConsumer> logger,
    IAzureAiTranslator azureAiTranslator) : IConsumer<ImportDeckEvent>
{
    public async Task Consume(ConsumeContext<ImportDeckEvent> context)
    {
        var deck = await dbContext.Decks.Include(deck => deck.User).FirstOrDefaultAsync(d => d.Id == context.Message.DeckId);

        if (deck == null)
        {
            logger.LogWarning("Associated deck {DeckId} not found. Skipping event", context.Message.DeckId);

            return;
        }

        var payload = deck.Export;

        if (payload == null)
        {
            logger.LogWarning("Associated deck's payload {DeckId} not found. Skipping event", context.Message.DeckId);

            return;
        }

        var verifiedItems = payload.Items.Where(x => !string.IsNullOrEmpty(x.Term) && !string.IsNullOrEmpty(x.Translation)).ToList();
        var notVerifiedItems = payload.Items.Where(x => string.IsNullOrWhiteSpace(x.Translation)).ToList();

       // List<Card> translatedVerifiedCards = []; 
        var translatedVerifiedCards = await AddTranslationForVerifiedCards(verifiedItems, payload, deck.Id, Guid.Parse(deck.UserId));
        var translatedNotVerifiedCards = await AddTranslationForMissingCards(notVerifiedItems, payload, deck.Id, Guid.Parse(deck.UserId));

        deck.Export!.Status = DeckExportStatus.Completed; 
        dbContext.Cards.AddRange(translatedVerifiedCards);
        dbContext.Cards.AddRange(translatedNotVerifiedCards);
        await dbContext.SaveChangesAsync();

        foreach (var c in translatedVerifiedCards.Concat(translatedNotVerifiedCards.Where(c => c.Status == CardProcessingStatus.LanguageDetected)))
        {
            await cardMessagePublisher.PublishNewCardForTasksCreating(c.Id);
        }
    }

    private async Task<List<Card>> AddTranslationForMissingCards(List<DeckExportItem> notVerifiedItems, DeckExportPayload payload, Guid deckId, Guid userId)
    {
        if (notVerifiedItems.Count == 0)
        {
            return[];
        }

        try
        {
            var translatedMissingItems =
                await azureAiTranslator.GetTranslations(notVerifiedItems.Select(x => x.Term).ToList(), [payload.LearnLanguage, payload.NativeLanguage]);

            var missingItemsCards = translatedMissingItems.Select(x =>
            {
                return new Card
                {
                    SourceLanguage = x.translated.SourceLanguage,
                    TargetLanguage = x.translated.DestinationLanguage,
                    NativeLanguage =
                        GetNativeLanguage(payload.LearnLanguage, payload.NativeLanguage, x.translated.SourceLanguage, x.translated.DestinationLanguage) ??
                        x.translated.DestinationLanguage,
                    LearnLanguage =
                        GetLearnLanguage(payload.LearnLanguage, payload.NativeLanguage, x.translated.SourceLanguage, x.translated.DestinationLanguage) ??
                        x.translated.SourceLanguage,
                    Id = Guid.NewGuid(),
                    DeckId = deckId,
                    Term = x.originalTerm,
                    Translations = x.translated.Translations.Select(t => new CardTranslationItem(t, "", "", 0, "")).ToList(),
                    Status = CardProcessingStatus.LanguageDetected,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    NextReviewDate = DateTime.UtcNow,
                    Level = "",
                    ReviewInterval = TimeSpan.FromMinutes(2),
                };
            }).ToList();

            return missingItemsCards;
        }
        catch (Exception e)
        {
            logger.LogError("Was not able to get translations for missing items. Cards will be saved without translations: {@MissingTranslationItems}", notVerifiedItems);   
            
            var missingItemsCards = notVerifiedItems.Select(x => new Card
            {
                NativeLanguage = payload.NativeLanguage,
                LearnLanguage = payload.LearnLanguage,
                Id = Guid.NewGuid(),
                DeckId = deckId,
                Term = x.Term,
                Translations = [],
                Level = "",
                Status = CardProcessingStatus.Failed,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                NextReviewDate = DateTime.UtcNow,
                ReviewInterval = TimeSpan.FromMinutes(2),
            }).ToList();

            return missingItemsCards;
        }

    }

    private async Task<List<Card>> AddTranslationForVerifiedCards(List<DeckExportItem> verifiedItems, DeckExportPayload payload, Guid deckId, Guid userId)
    {
        if (verifiedItems.Count == 0)
        {
            return [];
        }

    var parsedItems = await openAiGenerator.MapQuizletExportItemList(verifiedItems.Select(x => (x.Term, x.Translation)).ToList()!);

        var cards = parsedItems.Select(x =>
        {
            return new Card
            {
                SourceLanguage = x.SourceLanguage,
                TargetLanguage = x.DestinationLanguage,
                NativeLanguage =
                    GetNativeLanguage(payload.LearnLanguage, payload.NativeLanguage, x.SourceLanguage, x.DestinationLanguage) ?? x.DestinationLanguage,
                LearnLanguage = GetLearnLanguage(payload.LearnLanguage, payload.NativeLanguage, x.SourceLanguage, x.DestinationLanguage) ?? x.SourceLanguage,
                Id = Guid.NewGuid(),
                DeckId = deckId,
                Term = x.Term,
                Level = "",
                Translations = x.Translations.Select(t => new CardTranslationItem(t.Translation, "", t.Example, 0, "")).ToList(),
                Status = CardProcessingStatus.LanguageDetected,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                NextReviewDate = DateTime.UtcNow,
                ReviewInterval = TimeSpan.FromMinutes(2),
            };
        }).ToList();

        return cards;
    }

    private string? GetLearnLanguage(string? userProvidedLearnLanguage, string? userProvidedNativeLanguage, string? sourceLanguageDetected,
        string? targetLanguageDetected)
    {
        if (userProvidedLearnLanguage == sourceLanguageDetected || userProvidedLearnLanguage == targetLanguageDetected)
        {
            return userProvidedLearnLanguage;
        }

        if (userProvidedNativeLanguage == sourceLanguageDetected || userProvidedNativeLanguage == targetLanguageDetected)
        {
            return userProvidedNativeLanguage;
        }

        return sourceLanguageDetected;
    }

    private string? GetNativeLanguage(string? userProvidedLearnLanguage, string? userProvidedNativeLanguage, string? sourceLanguageDetected,
        string? targetLanguageDetected)
    {
        if (userProvidedNativeLanguage == sourceLanguageDetected || userProvidedNativeLanguage == targetLanguageDetected)
        {
            return userProvidedNativeLanguage;
        }

        if (userProvidedLearnLanguage == sourceLanguageDetected || userProvidedLearnLanguage == targetLanguageDetected)
        {
            return userProvidedLearnLanguage;
        }

        return targetLanguageDetected;
    }
}