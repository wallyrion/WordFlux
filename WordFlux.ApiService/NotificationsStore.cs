using System.Collections.Concurrent;

namespace WordFlux.ApiService;

public class NotificationsStore
{
    public List<NotificationSubscription> Notifications { get; set; } = [];


    public ConcurrentDictionary<Guid, DateTime> UserNotificationsHistory = [];
}