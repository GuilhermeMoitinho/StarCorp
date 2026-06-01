using StarCorp.Data.Enums;

namespace StarCorp.Data.Entities;

public record Payment(
    int Id,
    int BookingId,
    PaymentMethod Method,
    decimal Adjustment,
    decimal AmountPaid,
    DateTime PaidAt);
