using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WordFlux.Application.Jobs;

public class CardMessagePublisher(IServiceProvider serviceProvider, ILogger<CardDetectLanguageBackgroundJob> logger)
{
    private readonly Channel<Guid> _channelLanguageDetect = serviceProvider.GetRequiredKeyedService<Channel<Guid>>(Channels.CardDetectLanguage);
    private readonly Channel<Guid> _channelGenerateTasks = serviceProvider.GetRequiredKeyedService<Channel<Guid>>(Channels.CardCreateTasks);

    public async Task PublishNewCardForLanguageDetection(Guid cardId)
    {
        logger.LogInformation("Publishing new card for processing. CardId = {CardId}", cardId);
        await _channelLanguageDetect.Writer.WriteAsync(cardId);
    }
    
    public async Task PublishNewCardForTasksCreating(Guid cardId)
    {
        logger.LogInformation("Publishing new card for tasks creating. CardId = {CardId}", cardId);
        await _channelGenerateTasks.Writer.WriteAsync(cardId);
    }
}