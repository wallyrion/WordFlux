using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService.Ai;
using WordFlux.ApiService.Domain;
using WordFlux.ApiService.Persistence;

namespace WordFlux.ApiService.Jobs;

public class CardProcessingBackgroundJob(IServiceProvider serviceProvider, ILogger<CardProcessingBackgroundJob> logger, IOpenAiGenerator openAi)
    : BackgroundService
{
    private readonly Channel<Guid> _channel = serviceProvider.GetRequiredKeyedService<Channel<Guid>>(Channels.CardProcessing);

    private async Task PushNotProcessedMessagedToInitialQueue(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var nonProcessedCardIds = await dbContext.Cards.Where(c => c.Status == CardProcessingStatus.Unprocessed).Select(x => x.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var cardId in nonProcessedCardIds)
        {
            await _channel.Writer.WriteAsync(cardId, cancellationToken);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await PushNotProcessedMessagedToInitialQueue(stoppingToken);

        logger.LogInformation("Message job processing started.");

        // Continuously process messages from the channel until the service is stopped
        await foreach (var cardId in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            logger.LogInformation("Processing message for CardId: {CardId}", cardId);
            // Process one message at a time
            await ProcessMessageAsync(cardId, stoppingToken);
        }

        logger.LogInformation("Message job processing stopped.");
    }

    private async Task ProcessMessageAsync(Guid cardId, CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var card = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId, cancellationToken: stoppingToken);

            if (card != null)
            {
                card.Status = CardProcessingStatus.Failed;
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}

public class CardMessagePublisher(IServiceProvider serviceProvider, ILogger<CardProcessingBackgroundJob> logger)
{
    private readonly Channel<Guid> _channel = serviceProvider.GetRequiredKeyedService<Channel<Guid>>(Channels.CardProcessing);

    public async Task PublishNewCardForProcessing(Guid cardId)
    {
        logger.LogInformation("Publishing new card for processing. CardId = {CardId}", cardId);
        await _channel.Writer.WriteAsync(cardId);
    }
}