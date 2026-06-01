using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StarCorp.Data.Connection;
using StarCorp.Data.Setup;
using Testcontainers.MsSql;

namespace StarCorp.IntegrationTests;

// Sobe um SQL Server real em container, aplica o schema.sql e levanta a API contra ele.
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _db = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Troca a fabrica de conexao pela do container. ConfigureTestServices roda depois do registro da app,
        // entao sobrepoe a connection string do appsettings sem depender de ordem de configuracao.
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDbConnectionFactory>();
            services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(ConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        ConnectionString = new SqlConnectionStringBuilder(_db.GetConnectionString())
        {
            InitialCatalog = "StarCorp"
        }.ConnectionString;

        var scriptsDirectory = Path.Combine(AppContext.BaseDirectory, "db");
        var bootstrapper = new DatabaseBootstrapper(ConnectionString, scriptsDirectory);
        await bootstrapper.EnsureCreatedAsync(seed: false);
    }

    public new async Task DisposeAsync() => await _db.DisposeAsync();
}

[CollectionDefinition("api")]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>;
