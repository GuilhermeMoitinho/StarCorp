using FluentValidation;
using StarCorp.Business.Dtos;
using StarCorp.Business.Notifications.Abstractions;
using StarCorp.Business.Services.Abstractions;
using StarCorp.Data.Pagination;
using StarCorp.Data.Queries;
using StarCorp.Data.Repositories.Abstractions;

namespace StarCorp.Business.Services;

public sealed class FlightService(
    IFlightRepository flights,
    IValidator<FlightSearchRequest> validator,
    INotificationContext notifications) : IFlightService
{
    public async Task<PagedResult<FlightOfferDto>?> SearchAsync(FlightSearchRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                notifications.AddNotification(error.PropertyName, error.ErrorMessage);
            return null;
        }

        var criteria = new FlightSearchCriteria(
            Normalize(request.OriginCity),
            Normalize(request.DestinationCity),
            request.Date,
            request.MinPrice,
            request.MaxPrice,
            request.FareClass);

        var page = new PageQuery(request.Page, request.PageSize);
        var result = await flights.SearchAsync(criteria, page, ct);
        var items = result.Items.Select(ToDto).ToList();

        return new PagedResult<FlightOfferDto>(items, result.Page, result.PageSize, result.TotalItems);
    }

    private static FlightOfferDto ToDto(FlightOfferRow row) => new(
        row.FlightId, row.Airline, row.OriginCity, row.DestinationCity,
        row.DepartureUtc, row.ArrivalUtc, row.FareClassId, row.Price, row.SeatsAvailable);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
