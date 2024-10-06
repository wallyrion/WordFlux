using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService.Ai;
using WordFlux.ApiService.Domain;
using WordFlux.ApiService.Persistence;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Jobs;

public class CardCreateTasksBackgroundJob(IServiceProvider serviceProvider, ILogger<CardCreateTasksBackgroundJob> logger, IOpenAiGenerator openAi)
    : BackgroundService
{
    private readonly Channel<Guid> _channel = serviceProvider.GetRequiredKeyedService<Channel<Guid>>(Channels.CardCreateTasks);

    private async Task PushNotProcessedMessagedToInitialQueue(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var nonProcessedCardIds = await dbContext.Cards.Where(c => c.Status == CardProcessingStatus.LanguageDetected).Select(x => x.Id)
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
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            var card = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId, cancellationToken: stoppingToken);

            if (card == null)
            {
                logger.LogWarning("Skipping card that does not exist. CardId {CardId}", cardId);

                return;
            }

            if (card.LearnLanguage == null || card.NativeLanguage == null)
            {
                card.Status = CardProcessingStatus.CardExampleTaskCreated;
                await dbContext.SaveChangesAsync(stoppingToken);
                
                logger.LogWarning("Skipping card that does not have detected languages. CardId {CardId}", cardId);
                    return;
            }
            
            if (card.LearnLanguage != card.SourceLanguage)
            {
                card.Status = CardProcessingStatus.CardExampleTaskCreated;
                await dbContext.SaveChangesAsync(stoppingToken);
                
                logger.LogWarning("Skipping card as source term not in learning language. CardId {CardId}", cardId);
                    return;
            }
            

            var examples = (await openAi.GetExamplesCardTask(card.Term, card.LearnLanguage!, card.NativeLanguage!, 10, stoppingToken)) ?? [];
            card.ExampleTasks = examples.Select(x => new CardTaskExample
            {
                ExampleLearn = x.ExampleLearn,
                ExampleNative = x.ExampleNative
            }).ToList();

            card.Status = CardProcessingStatus.CardExampleTaskCreated;
            await dbContext.SaveChangesAsync(stoppingToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while processing cardId {CardId}", cardId);
            
            var card = await dbContext.Cards.FirstOrDefaultAsync(x => x.Id == cardId, cancellationToken: stoppingToken);

            if (card != null)
            {
                card.Status = CardProcessingStatus.Failed;
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
