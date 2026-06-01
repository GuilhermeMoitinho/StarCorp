using StarCorp.Business.Entities;
using StarCorp.Business.Pagination;
using StarCorp.Business.Queries;

namespace StarCorp.Business.Repositories.Abstractions;

public interface IFlightRepository
{
    Task<Flight?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResult<FlightOfferRow>> SearchAsync(FlightSearchCriteria criteria, PageQuery page, CancellationToken ct);
}
