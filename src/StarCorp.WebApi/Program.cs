using StarCorp.WebApi.Configurations;
using StarCorp.WebApi.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAppMvc()
    .AddAppSwagger()
    .AddAppDatabase(builder.Configuration)
    .AddAppDependencies()
    .AddAppHealthChecks();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.UseAppSwagger();

app.MapControllers();
app.MapAppHealthChecks();

if (!app.Environment.IsEnvironment("Testing"))
    await app.UseAppDatabaseAsync();

app.Run();

public partial class Program { }
