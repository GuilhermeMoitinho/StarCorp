namespace StarCorp.Business.Notifications.Abstractions;

// O valor inteiro de cada tipo e o proprio status HTTP, entao o controller mapeia direto.
public enum NotificationType
{
    None = 0,
    Validation = 400,
    NotFound = 404,
    Conflict = 409,
    UnprocessableEntity = 422
}

public interface INotificationContext
{
    IReadOnlyCollection<Notification> Notifications { get; }
    bool HasNotifications { get; }
    NotificationType Type { get; }

    void AddNotification(string key, string message);
    void AddNotFound(string key, string message);
    void AddConflict(string key, string message);
    void AddUnprocessable(string key, string message);
}
