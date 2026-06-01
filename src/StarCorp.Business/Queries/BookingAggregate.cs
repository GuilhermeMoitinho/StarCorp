using StarCorp.Business.Entities;

namespace StarCorp.Business.Queries;

public record BookingAggregate(
    Booking Booking,
    Flight Flight,
    IReadOnlyList<BookingPassenger> Passengers,
    Payment? Payment,
    Cancellation? Cancellation);
