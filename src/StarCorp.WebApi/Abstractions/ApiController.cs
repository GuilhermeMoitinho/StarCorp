using Microsoft.AspNetCore.Mvc;
using StarCorp.Business.Notifications.Abstractions;

namespace StarCorp.WebApi.Abstractions;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiController(INotificationContext notifications) : ControllerBase
{
    protected INotificationContext Notifications { get; } = notifications;

    // Sem notificacao -> resposta de sucesso. Com notificacao -> status mapeado pelo tipo do erro.
    protected IActionResult Respond(object? payload = null, int successStatus = StatusCodes.Status200OK)
    {
        if (!Notifications.HasNotifications)
            return payload is null ? StatusCode(successStatus) : StatusCode(successStatus, payload);

        var problem = new
        {
            status = (int)Notifications.Type,
            errors = Notifications.Notifications.Select(n => new { n.Key, n.Message })
        };

        return Notifications.Type switch
        {
            NotificationType.NotFound => NotFound(problem),
            NotificationType.Conflict => Conflict(problem),
            NotificationType.UnprocessableEntity => UnprocessableEntity(problem),
            _ => BadRequest(problem)
        };
    }
}
