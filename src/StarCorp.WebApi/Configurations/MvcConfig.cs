using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace StarCorp.WebApi.Configurations;

public static class MvcConfig
{
    public static IServiceCollection AddAppMvc(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry => entry.Value!.Errors
                        .Select(error => new { Key = entry.Key, Message = error.ErrorMessage }))
                    .ToList();

                var problem = new { status = StatusCodes.Status400BadRequest, errors };
                return new BadRequestObjectResult(problem);
            };
        });

        return services;
    }
}
