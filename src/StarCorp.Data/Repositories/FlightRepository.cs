using Dapper;
using StarCorp.Data.Connection;
using StarCorp.Business.Entities;
using StarCorp.Business.Pagination;
using StarCorp.Business.Queries;
using StarCorp.Business.Repositories.Abstractions;

namespace StarCorp.Data.Repositories;

public sealed class FlightRepository(IDbConnectionFactory factory) : IFlightRepository
{
    public async Task<Flight?> GetByIdAsync(int id, CancellationToken ct)
    {
        await using var conn = factory.Create();
        var command = new CommandDefinition(
            "SELECT Id, AirlineId, OriginCity, DestinationCity, DepartureUtc, ArrivalUtc, BasePrice FROM Flights WHERE Id = @id;",
            new { id }, cancellationToken: ct);
        return await conn.QuerySingleOrDefaultAsync<Flight>(command);
    }

    public async Task<PagedResult<FlightOfferRow>> SearchAsync(FlightSearchCriteria criteria, PageQuery page, CancellationToken ct)
    {
        const string fromAndWhere = @"
FROM Flights f
JOIN Airlines a ON a.Id = f.AirlineId
JOIN FlightSeats fs ON fs.FlightId = f.Id
JOIN FareClasses fc ON fc.Id = fs.FareClassId
WHERE (@OriginCity IS NULL OR f.OriginCity = @OriginCity)
  AND (@DestinationCity IS NULL OR f.DestinationCity = @DestinationCity)
  AND (@Date IS NULL OR CAST(f.DepartureUtc AS DATE) = @Date)
  AND (@FareClassId IS NULL OR fc.Id = @FareClassId)
  AND (@MinPrice IS NULL OR f.BasePrice * fc.PriceMultiplier >= @MinPrice)
  AND (@MaxPrice IS NULL OR f.BasePrice * fc.PriceMultiplier <= @MaxPrice)";

        var sql = $@"
SELECT COUNT(*) {fromAndWhere};

SELECT
    f.Id              AS FlightId,
    a.Name            AS Airline,
    f.OriginCity      AS OriginCity,
    f.DestinationCity AS DestinationCity,
    f.DepartureUtc    AS DepartureUtc,
    f.ArrivalUtc      AS ArrivalUtc,
    fc.Id             AS FareClassId,
    CAST(f.BasePrice * fc.PriceMultiplier AS DECIMAL(18,2)) AS Price,
    fs.SeatsAvailable AS SeatsAvailable
{fromAndWhere}
ORDER BY f.DepartureUtc, Price
OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;";

        var parameters = new
        {
            criteria.OriginCity,
            criteria.DestinationCity,
            criteria.Date,
            FareClassId = (byte?)criteria.FareClass,
            criteria.MinPrice,
            criteria.MaxPrice,
            page.Skip,
            PageSize = page.NormalizedPageSize
        };

        await using var conn = factory.Create();
        await conn.OpenAsync(ct);
        using var multi = await conn.QueryMultipleAsync(new CommandDefinition(sql, parameters, cancellationToken: ct));
        var total = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<FlightOfferRow>()).ToList();
        return new PagedResult<FlightOfferRow>(items, page.NormalizedPage, page.NormalizedPageSize, total);
    }
}
