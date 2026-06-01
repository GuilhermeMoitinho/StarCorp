using Microsoft.OpenApi;

namespace StarCorp.WebApi.Configurations;

public static class SwaggerConfig
{
    public static IServiceCollection AddAppSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "StarCorp Travel API",
                Version = "v1",
                Description = "API de reserva de passagens aereas."
            }));
        return services;
    }

    public static WebApplication UseAppSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}
