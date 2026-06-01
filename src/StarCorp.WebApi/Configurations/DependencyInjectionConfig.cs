using FluentValidation;
using StarCorp.Business.Dtos;
using StarCorp.Business.Notifications;
using StarCorp.Business.Notifications.Abstractions;
using StarCorp.Business.Pricing;
using StarCorp.Business.Pricing.Abstractions;
using StarCorp.Business.Services;
using StarCorp.Business.Services.Abstractions;
using StarCorp.Business.Validators;
using StarCorp.Data.Repositories;
using StarCorp.Data.Repositories.Abstractions;

namespace StarCorp.WebApi.Configurations;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddAppDependencies(this IServiceCollection services)
    {
        // Scoped: service e controller compartilham a mesma instancia de notificacao por request.
        services.AddScoped<INotificationContext, NotificationContext>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IFareClassRepository, FareClassRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddScoped<IFlightService, FlightService>();
        services.AddScoped<IBookingService, BookingService>();

        // Calculadoras sao puras e sem estado.
        services.AddSingleton<IPricingCalculator, PricingCalculator>();
        services.AddSingleton<ICancellationPolicy, CancellationPolicy>();
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IValidator<FlightSearchRequest>, FlightSearchValidator>();
        services.AddScoped<IValidator<CreateBookingRequest>, CreateBookingValidator>();
        services.AddScoped<IValidator<PaymentRequest>, PaymentValidator>();

        return services;
    }
}
