using StarCorp.Business.Enums;

namespace StarCorp.Business.Queries;

public record FlightSearchCriteria(
    string? OriginCity,
    string? DestinationCity,
    DateOnly? Date,
    decimal? MinPrice,
    decimal? MaxPrice,
    FareClass? FareClass);
