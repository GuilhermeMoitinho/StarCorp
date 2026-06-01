using StarCorp.Data.Enums;

namespace StarCorp.Data.Repositories;

public record NewBooking(
    int CustomerId,
    int FlightId,
    FareClass FareClassId,
    int PassengerCount,
    decimal FarePrice,
    decimal Subtotal,
    decimal Taxes,
    decimal ServiceFee,
    decimal AmountDue,
    DateTime CreatedAtUtc);

public record NewPassenger(string Name, string Document);

public record NewPayment(PaymentMethod Method, decimal Adjustment, decimal AmountPaid, DateTime PaidAtUtc);
