using StarCorp.Data.Enums;

namespace StarCorp.Data.Queries;

public record FlightOfferRow(
    int FlightId,
    string Airline,
    string OriginCity,
    string DestinationCity,
    DateTime DepartureUtc,
    DateTime ArrivalUtc,
    FareClass FareClassId,
    decimal Price,
    int SeatsAvailable);
