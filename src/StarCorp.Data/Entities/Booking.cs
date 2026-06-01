using StarCorp.Data.Enums;

namespace StarCorp.Data.Entities;

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
