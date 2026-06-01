namespace StarCorp.Data.Entities;

public record Cancellation(
    int Id,
    int BookingId,
    decimal RefundPercentage,
    decimal RefundAmount,
    DateTime CancelledAt);
