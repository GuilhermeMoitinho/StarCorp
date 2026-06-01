using StarCorp.Data.Entities;

namespace StarCorp.Data.Queries;

public record BookingAggregate(
    Booking Booking,
    Flight Flight,
    IReadOnlyList<BookingPassenger> Passengers,
    Payment? Payment,
    Cancellation? Cancellation);
