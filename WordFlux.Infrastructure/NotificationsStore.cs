using System.Collections.Concurrent;
using WordFlux.Domain;

namespace WordFlux.Infrastructure;

public class NotificationsStore
{
    public List<NotificationSubscription> Notifications { get; set; } = [];


    public ConcurrentDictionary<Guid, DateTime> UserNotificationsHistory = [];
}