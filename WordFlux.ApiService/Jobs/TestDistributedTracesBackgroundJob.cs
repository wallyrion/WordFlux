using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using WordFlux.ApiService.Ai;
using WordFlux.ApiService.Domain;
using WordFlux.ApiService.Persistence;
using WordFlux.ApiService.Services;
using WordFlux.Contracts;

namespace WordFlux.ApiService.Jobs;

public class TestDistributedTracesBackgroundJob(IServiceProvider serviceProvider, ILogger<TestDistributedTracesBackgroundJob> logger, [FromKeyedServices("AzureAiTranslator")] ITranslationService translationService)
    : BackgroundService
{
    private static ActivitySource source = new ActivitySource("Sample.DistributedTracing", "1.0.0");

    public static readonly Channel<(Guid messageid, Dictionary<string, object> metadata)> MyChannel = Channel.CreateUnbounded<(Guid, Dictionary<string, object>)>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Continuously process messages from the channel until the service is stopped
        await foreach (var message in MyChannel.Reader.ReadAllAsync(stoppingToken))
        {
            var messageActivity = message.metadata["activity"] as Activity;

            using (Activity activity = source.StartActivity("SomeWork", ActivityKind.Consumer, messageActivity.Context)) 
            {
                
                logger.LogInformation("Message job processing started for ChannelMessageId = {ChannelMessageId}", message.messageid);

                
                var test = await translationService.GetTranslations("test", ["en", "ru"]);

                await using var scope = serviceProvider.CreateAsyncScope();
                await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var card = await context.Cards.FirstOrDefaultAsync(cancellationToken: stoppingToken);
                
                logger.LogInformation("Message job processing finished for ChannelMessageId = {ChannelMessageId}", message.messageid);
            }
        }
    }
}