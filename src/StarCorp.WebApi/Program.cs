using StarCorp.WebApi.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAppMvc()
    .AddAppSwagger()
    .AddAppDatabase(builder.Configuration)
    .AddAppDependencies()
    .AddAppHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseAppSwagger();

app.MapControllers();
app.MapAppHealthChecks();

// Em ambiente de teste a fabrica de integracao cuida do banco; fora dele, garante o schema (e seed em Development).
if (!app.Environment.IsEnvironment("Testing"))
    await app.UseAppDatabaseAsync();

app.Run();

public partial class Program { }
