using StarCorp.Business.Notifications.Abstractions;

namespace StarCorp.Business.Notifications;

public sealed class NotificationContext : INotificationContext
{
    private readonly List<Notification> _notifications = [];

    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();
    public bool HasNotifications => _notifications.Count > 0;
    public NotificationType Type { get; private set; } = NotificationType.None;

    public void AddNotification(string key, string message)
    {
        if (Type == NotificationType.None)
            Type = NotificationType.Validation;
        _notifications.Add(new Notification(key, message));
    }

    public void AddNotFound(string key, string message) => Add(NotificationType.NotFound, key, message);

    public void AddConflict(string key, string message) => Add(NotificationType.Conflict, key, message);

    public void AddUnprocessable(string key, string message) => Add(NotificationType.UnprocessableEntity, key, message);

    private void Add(NotificationType type, string key, string message)
    {
        Type = type;
        _notifications.Add(new Notification(key, message));
    }
}
