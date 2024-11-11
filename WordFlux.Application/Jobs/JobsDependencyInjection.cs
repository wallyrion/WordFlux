using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace WordFlux.Application.Jobs;

public static class JobsDependencyInjection
{
    public static IServiceCollection AddChannels(this IServiceCollection services)
    {
        Channel<Guid> channelDetectLanguage = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
        
        Channel<Guid> channelCreateTasks = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
        
        services.AddKeyedSingleton(Channels.CardDetectLanguage, channelDetectLanguage);      
        services.AddKeyedSingleton(Channels.CardCreateTasks, channelCreateTasks);
        
        services.AddHostedService<CardDetectLanguageBackgroundJob>();
        services.AddHostedService<CardCreateTasksBackgroundJob>();
        services.AddHostedService<TestDistributedTracesBackgroundJob>();
        services.AddSingleton<CardMessagePublisher>();
        
        return services;
    }
}