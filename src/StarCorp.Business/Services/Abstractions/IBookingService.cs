using StarCorp.Business.Dtos;

namespace StarCorp.Business.Services.Abstractions;

public interface IBookingService
{
    Task<BookingResponseDto?> CreateAsync(CreateBookingRequest request, CancellationToken ct);
    Task<BookingResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PaymentResponseDto?> PayAsync(int bookingId, PaymentRequest request, CancellationToken ct);
    Task<CancellationResponseDto?> CancelAsync(int bookingId, CancellationToken ct);
}
