using Microsoft.AspNetCore.Diagnostics;

namespace StarCorp.WebApi.Errors;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Erro nao tratado ao processar a requisicao.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            status = StatusCodes.Status500InternalServerError,
            errors = new[] { new { key = "server", message = "Erro interno ao processar a requisicao." } }
        }, cancellationToken);

        return true;
    }
}
