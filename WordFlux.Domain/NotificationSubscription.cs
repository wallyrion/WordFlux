namespace WordFlux.Domain;

public class NotificationSubscription
{
    public Guid? Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Url { get; set; }

    public string? P256dh { get; set; }

    public string? Auth { get; set; }
}