using StarCorp.WebApi.HealthChecks;

namespace StarCorp.WebApi.Configurations;

public static class HealthChecksConfig
{
    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks().AddCheck<SqlHealthCheck>("sqlserver");
        return services;
    }

    public static WebApplication MapAppHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}
