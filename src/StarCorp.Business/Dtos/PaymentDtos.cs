using StarCorp.Business.Enums;

namespace StarCorp.Business.Dtos;

public record PaymentRequest(PaymentMethod Method);

public record PaymentResponseDto(
    int BookingId,
    PaymentMethod Method,
    decimal AmountDue,
    decimal Adjustment,
    decimal AmountPaid,
    DateTime PaidAt);

public record CancellationResponseDto(
    int BookingId,
    decimal RefundPercentage,
    decimal RefundAmount,
    DateTime CancelledAt);
