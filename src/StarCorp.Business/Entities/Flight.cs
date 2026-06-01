namespace StarCorp.Business.Entities;

public record Flight(
    int Id,
    int AirlineId,
    string OriginCity,
    string DestinationCity,
    DateTime DepartureUtc,
    DateTime ArrivalUtc,
    decimal BasePrice);
