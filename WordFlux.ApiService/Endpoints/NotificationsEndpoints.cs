using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebPush;

namespace WordFlux.ApiService.Endpoints;

public static class NotificationsEndpoints
{
    public static WebApplication MapPushNotificationsEndpoints(this WebApplication app)
    {
        app.MapGet("/notifications", async ([FromServices] NotificationsStore store, ILogger<Program> logger) => { return store.Notifications; });

        app.MapGet("/notifications-by-url", async ([FromServices] NotificationsStore store, string url) =>
        {
            return store.Notifications.FirstOrDefault(x => x.Url == url);
        });

        
        app.MapPost("/notifications/clear", async ([FromServices] NotificationsStore store, ILogger<Program> logger) => { store.Notifications = []; });

        
        app.MapPost("/notifications", async (NotificationSubscription subscription, [FromServices] NotificationsStore store, ILogger<Program> logger,
            ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
        {
            var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

            subscription.UserId = userId;
            subscription.Id = Guid.NewGuid();
            
            var existingNotificationForDevice = store.Notifications.FirstOrDefault(x => x.UserId == userId && x.Url == subscription.Url);

            if (existingNotificationForDevice != null)
            {
                return existingNotificationForDevice;
            }

            store.Notifications.Add(subscription);

            return subscription;

        });
        
        app.MapDelete("/notifications/{subscriptionId}", async (Guid subscriptionId, [FromServices] NotificationsStore store, ILogger<Program> logger,
            ClaimsPrincipal claimsPrincipal, UserManager<AppUser> userManager) =>
        {
            var userId = Guid.Parse(userManager.GetUserId(claimsPrincipal)!);

            var existingSubscription = store.Notifications.Find(x => x.UserId == userId && x.Id == subscriptionId);

            if (existingSubscription != null)
            {
                store.Notifications.Remove(existingSubscription);
            }
        });
        
        app.MapPost("/send-test-notifications", async ([FromServices] NotificationsStore store, ILogger<Program> logger) =>
        {
            foreach (var subscription in store.Notifications)
            {
                var publicKey = "BLC8GOevpcpjQiLkO7JmVClQjycvTCYWm6Cq_a7wJZlstGTVZvwGFFHMYfXt6Njyvgx_GlXJeo5cSiZ1y4JOx1o";
                var privateKey = "OrubzSz3yWACscZXjFQrrtDwCKg-TGFuWhluQ2wLXDo";

                var pushSubscription = new PushSubscription(subscription.Url, subscription.P256dh, subscription.Auth);
                logger.LogInformation("Pushing notification. Details: {@Details}", pushSubscription);

                var vapidDetails = new VapidDetails("mailto:kornienko1296@gmail.com", publicKey, privateKey);
                var webPushClient = new WebPushClient();

                try
                {
                    var payload = JsonSerializer.Serialize(new
                    {
                        message = "test message",
                        url = $"myorders/{Guid.NewGuid()}",
                    });

                    await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "error while sending push notificaiton. Details: {@Details}", JsonSerializer.Serialize(subscription));
                    Console.Error.WriteLine("Error sending push notification: " + ex.Message);
                }
            }
        });


        return app;
    }
}