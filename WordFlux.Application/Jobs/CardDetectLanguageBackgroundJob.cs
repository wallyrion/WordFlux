﻿using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordFlux.Domain;
using WordFlux.Domain.Domain;

namespace WordFlux.Application.Jobs;

public class CardDetectLanguageBackgroundJob(IServiceProvider serviceProvider, ILogger<CardDetectLanguageBackgroundJob> logger, IOpenAiGenerator openAi)
    : BackgroundService
{
    private readonly Channel<Guid> _channelDetectLanguage = serviceProvider.GetRequiredKeyedService<Channel<Guid>>(Channels.CardDetectLanguage);

    private async Task PushNotProcessedMessagedToInitialQueue(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();

        var nonProcessedCardIds = await dbContext.Cards.Where(c => c.Status == CardProcessingStatus.Unprocessed).Select(x => x.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var cardId in nonProcessedCardIds)
        {
            await _channelDetectLanguage.Writer.WriteAsync(cardId, cancellationToken);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await PushNotProcessedMessagedToInitialQueue(stoppingToken);

        logger.LogInformation("Message job processing started");

        // Continuously process messages from the channel until the service is stopped
        await foreach (var cardId in _channelDetectLanguage.Reader.ReadAllAsync(stoppingToken))
        {
            logger.LogInformation("Processing message for CardId: {CardId}", cardId);
            // Process one message at a time
            await ProcessMessageAsync(cardId, stoppingToken);
        }

        logger.LogInformation("Message job processing stopped");
    }

    private async Task ProcessMessageAsync(Guid cardId, CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();

            var card = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId, cancellationToken: stoppingToken);

            if (card == null)
            {
                logger.LogWarning("Skipping card that does not exist. CardId {CardId}", cardId);

                return;
            }

            if (card.SourceLanguage == null || card.TargetLanguage == null)
            {
                var translation = card.Translations.FirstOrDefault()?.Term ?? "";
                var languages = await openAi.DetectLanguage(card.Term, translation, stoppingToken);

                // will be removed
                card.SourceLanguage = languages.Value.sourceLanguage == "" ? null : languages.Value.sourceLanguage;
                card.TargetLanguage = languages.Value.destinationLanguage == "" ? null : languages.Value.destinationLanguage;;

                if (card.SourceLanguage == "en")
                {
                    card.NativeLanguage = card.TargetLanguage;
                    card.LearnLanguage = "en";
                }

                if (card.TargetLanguage == "en")
                {
                    card.NativeLanguage = card.SourceLanguage;
                    card.LearnLanguage = "en";
                }
            }
            
            card.Status = CardProcessingStatus.LanguageDetected;

            await dbContext.SaveChangesAsync(stoppingToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while processing cardId {CardId}", cardId);
            
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();

            var card = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId, cancellationToken: stoppingToken);

            if (card != null)
            {
                card.Status = CardProcessingStatus.Failed;
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}