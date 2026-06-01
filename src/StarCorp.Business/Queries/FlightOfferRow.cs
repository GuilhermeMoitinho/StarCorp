using StarCorp.Business.Enums;

namespace StarCorp.Business.Queries;

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
