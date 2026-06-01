using StarCorp.Business.Enums;

namespace StarCorp.Business.Entities;

public record Payment(
    int Id,
    int BookingId,
    PaymentMethod Method,
    decimal Adjustment,
    decimal AmountPaid,
    DateTime PaidAt);
