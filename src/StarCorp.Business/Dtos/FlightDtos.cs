using StarCorp.Business.Enums;

namespace StarCorp.Business.Dtos;

public record FlightSearchRequest(
    string? OriginCity = null,
    string? DestinationCity = null,
    DateOnly? Date = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    FareClass? FareClass = null,
    int Page = 1,
    int PageSize = 10);

public record FlightOfferDto(
    int FlightId,
    string Airline,
    string OriginCity,
    string DestinationCity,
    DateTime DepartureUtc,
    DateTime ArrivalUtc,
    FareClass FareClass,
    decimal Price,
    int SeatsAvailable);
