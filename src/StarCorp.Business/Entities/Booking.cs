using StarCorp.Business.Enums;

namespace StarCorp.Business.Entities;

public record Booking(
    int Id,
    int CustomerId,
    int FlightId,
    FareClass FareClassId,
    BookingStatus Status,
    int PassengerCount,
    decimal FarePrice,
    decimal Subtotal,
    decimal Taxes,
    decimal ServiceFee,
    decimal AmountDue,
    DateTime CreatedAt);
