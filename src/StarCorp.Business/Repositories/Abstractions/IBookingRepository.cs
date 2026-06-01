using StarCorp.Business.Entities;
using StarCorp.Business.Queries;

namespace StarCorp.Business.Repositories.Abstractions;

public interface IBookingRepository
{
    Task<int?> CreateAsync(NewBooking booking, IReadOnlyList<NewPassenger> passengers, CancellationToken ct);

    Task<BookingAggregate?> GetAggregateAsync(int id, CancellationToken ct);

    Task<bool> RegisterPaymentAsync(int bookingId, NewPayment payment, CancellationToken ct);

    Task<bool> CancelAsync(Booking booking, decimal refundPercentage, decimal refundAmount, DateTime cancelledAtUtc, CancellationToken ct);
}
