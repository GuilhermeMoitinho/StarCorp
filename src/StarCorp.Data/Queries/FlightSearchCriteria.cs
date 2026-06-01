using StarCorp.Data.Enums;

namespace StarCorp.Data.Queries;

public record FlightSearchCriteria(
    string? OriginCity,
    string? DestinationCity,
    DateOnly? Date,
    decimal? MinPrice,
    decimal? MaxPrice,
    FareClass? FareClass);
