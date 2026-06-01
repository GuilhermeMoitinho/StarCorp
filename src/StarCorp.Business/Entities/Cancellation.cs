namespace StarCorp.Business.Entities;

public record Cancellation(
    int Id,
    int BookingId,
    decimal RefundPercentage,
    decimal RefundAmount,
    DateTime CancelledAt);
