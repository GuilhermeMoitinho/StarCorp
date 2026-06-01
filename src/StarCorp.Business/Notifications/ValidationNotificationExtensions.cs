using FluentValidation;
using StarCorp.Business.Notifications.Abstractions;

namespace StarCorp.Business.Notifications;

public static class ValidationNotificationExtensions
{
    public static async Task<bool> EnsureValidAsync<T>(
        this INotificationContext notifications, IValidator<T> validator, T instance, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid)
            return true;

        foreach (var error in result.Errors)
            notifications.AddNotification(error.PropertyName, error.ErrorMessage);
        return false;
    }
}
