using StarCorp.Business.Dtos;
using StarCorp.Business.Pagination;

namespace StarCorp.Business.Services.Abstractions;

public interface IFlightService
{
    Task<PagedResult<FlightOfferDto>?> SearchAsync(FlightSearchRequest request, CancellationToken ct);
}
