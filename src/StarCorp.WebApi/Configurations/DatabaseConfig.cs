using StarCorp.Data.Connection;
using StarCorp.Data.Setup;

namespace StarCorp.WebApi.Configurations;

public static class DatabaseConfig
{
    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' nao configurada.");

        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));

        var scriptsDirectory = configuration["Database:ScriptsPath"];
        if (string.IsNullOrWhiteSpace(scriptsDirectory))
            scriptsDirectory = Path.Combine(AppContext.BaseDirectory, "db");

        services.AddSingleton(new DatabaseBootstrapper(connectionString, scriptsDirectory));
        return services;
    }

    public static async Task UseAppDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var bootstrapper = scope.ServiceProvider.GetRequiredService<DatabaseBootstrapper>();
        await bootstrapper.EnsureCreatedAsync(seed: app.Environment.IsDevelopment());
    }
}
