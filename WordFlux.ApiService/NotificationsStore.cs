using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebPush;
using WordFlux.ApiService.Domain;
using WordFlux.ApiService.Persistence;

namespace WordFlux.ApiService;

public class NotificationsStore
{
    public List<NotificationSubscription> Notifications { get; set; } = [];


    public ConcurrentDictionary<Guid, DateTime> UserNotificationsHistory = [];
}

public class TestBackgroundService(IServiceProvider services, NotificationsStore notificationsStore, ILogger<BackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            await using var scope = services.CreateAsyncScope();

            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var users = dbContext.Users.ToList();

            foreach (var user in users)
            {
                await NotifyAboutNextCardAppeared(dbContext, Guid.Parse(user.Id));
            }
        }
    }

    private async Task NotifyAboutNextCardAppeared(ApplicationDbContext dbContext, Guid userId)
    {
        var nextCard = await dbContext.Cards
            .Where(c => c.CreatedBy == userId && c.NextReviewDate <= DateTime.UtcNow)
            .OrderBy(x => x.NextReviewDate)
            .FirstOrDefaultAsync();

        if (nextCard == null)
        {
            return;
        }

        if (nextCard.NextReviewDate > DateTime.UtcNow)
        {
            return;
        }

        if (notificationsStore.UserNotificationsHistory.TryGetValue(userId, out var lastUserNotificationDate) && DateTime.UtcNow - lastUserNotificationDate < TimeSpan.FromMinutes(1))
        {
            return;
        }
        
        var userNotifications = notificationsStore.Notifications.Where(s => s.UserId == userId);

        foreach (var subscription in userNotifications)
        {
            await SendNotificationsAsync(subscription, nextCard);
        }
    }

    private async Task SendNotificationsAsync(NotificationSubscription subscription, Card card)
    {
        var publicKey = "BLC8GOevpcpjQiLkO7JmVClQjycvTCYWm6Cq_a7wJZlstGTVZvwGFFHMYfXt6Njyvgx_GlXJeo5cSiZ1y4JOx1o";
        var privateKey = "OrubzSz3yWACscZXjFQrrtDwCKg-TGFuWhluQ2wLXDo";
        
        var pushSubscription = new PushSubscription(subscription.Url, subscription.P256dh, subscription.Auth);
        logger.LogInformation("Pushing notification from background service. Details: {@Details}", pushSubscription);
        
        var vapidDetails = new VapidDetails("mailto:kornienko1296@gmail.com", publicKey, privateKey);
        var webPushClient = new WebPushClient();

        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                message = "New card appeared",
                url = $"myorders/{Guid.NewGuid()}",
            });

            await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);

            notificationsStore.UserNotificationsHistory.AddOrUpdate(subscription.UserId.Value, _ => DateTime.UtcNow, (_, _) => DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while sending push notificaiton. Details: {@Details}", JsonSerializer.Serialize(subscription));
            Console.Error.WriteLine("Error sending push notification: " + ex.Message);
        }
    }
}