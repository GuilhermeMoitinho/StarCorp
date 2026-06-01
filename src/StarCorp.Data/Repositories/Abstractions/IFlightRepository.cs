using StarCorp.Data.Entities;
using StarCorp.Data.Pagination;
using StarCorp.Data.Queries;

namespace StarCorp.Data.Repositories.Abstractions;

public interface IFlightRepository
{
    Task<Flight?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResult<FlightOfferRow>> SearchAsync(FlightSearchCriteria criteria, PageQuery page, CancellationToken ct);
}
