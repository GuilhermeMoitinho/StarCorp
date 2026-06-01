using FluentValidation;
using StarCorp.Business.Dtos;
using StarCorp.Business.Notifications.Abstractions;
using StarCorp.Business.Pricing.Abstractions;
using StarCorp.Business.Services.Abstractions;
using StarCorp.Data.Enums;
using StarCorp.Data.Queries;
using StarCorp.Data.Repositories;
using StarCorp.Data.Repositories.Abstractions;

namespace StarCorp.Business.Services;

public sealed class BookingService(
    IBookingRepository bookings,
    ICustomerRepository customers,
    IFlightRepository flights,
    IFareClassRepository fareClasses,
    IPricingCalculator pricing,
    ICancellationPolicy cancellationPolicy,
    INotificationContext notifications,
    IValidator<CreateBookingRequest> createValidator,
    IValidator<PaymentRequest> paymentValidator,
    TimeProvider clock) : IBookingService
{
    public async Task<BookingResponseDto?> CreateAsync(CreateBookingRequest request, CancellationToken ct)
    {
        if (!await ValidateAsync(createValidator, request, ct))
            return null;

        var customer = await customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
        {
            notifications.AddNotFound("customer", "Cliente nao encontrado.");
            return null;
        }

        if (!customer.IsActive)
        {
            notifications.AddUnprocessable("customer", "Cliente inativo nao pode realizar reservas.");
            return null;
        }

        var flight = await flights.GetByIdAsync(request.FlightId, ct);
        if (flight is null)
        {
            notifications.AddNotFound("flight", "Voo nao encontrado.");
            return null;
        }

        var fareClass = await fareClasses.GetByIdAsync(request.FareClass, ct);
        if (fareClass is null)
        {
            notifications.AddNotFound("fareClass", "Classe tarifaria invalida.");
            return null;
        }

        var passengerCount = request.Passengers.Count;
        var breakdown = pricing.Calculate(flight.BasePrice, fareClass.PriceMultiplier, passengerCount);

        var newBooking = new NewBooking(
            customer.Id, flight.Id, request.FareClass, passengerCount,
            breakdown.FarePrice, breakdown.Subtotal, breakdown.Taxes, breakdown.ServiceFee, breakdown.AmountDue,
            clock.GetUtcNow().UtcDateTime);

        var passengers = request.Passengers
            .Select(p => new NewPassenger(p.Name.Trim(), p.Document.Trim()))
            .ToList();

        var bookingId = await bookings.CreateAsync(newBooking, passengers, ct);
        if (bookingId is null)
        {
            notifications.AddConflict("flight", "Nao ha assentos disponiveis na classe escolhida.");
            return null;
        }

        var aggregate = await bookings.GetAggregateAsync(bookingId.Value, ct);
        return ToResponse(aggregate!);
    }

    public async Task<BookingResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var aggregate = await bookings.GetAggregateAsync(id, ct);
        if (aggregate is null)
        {
            notifications.AddNotFound("booking", "Reserva nao encontrada.");
            return null;
        }

        return ToResponse(aggregate);
    }

    public async Task<PaymentResponseDto?> PayAsync(int bookingId, PaymentRequest request, CancellationToken ct)
    {
        if (!await ValidateAsync(paymentValidator, request, ct))
            return null;

        var aggregate = await bookings.GetAggregateAsync(bookingId, ct);
        if (aggregate is null)
        {
            notifications.AddNotFound("booking", "Reserva nao encontrada.");
            return null;
        }

        if (aggregate.Booking.Status == BookingStatus.Cancelled)
        {
            notifications.AddConflict("booking", "Reserva cancelada nao pode ser paga.");
            return null;
        }

        if (aggregate.Booking.Status == BookingStatus.Paid)
        {
            notifications.AddConflict("booking", "Reserva ja foi paga.");
            return null;
        }

        var charge = pricing.ApplyPayment(aggregate.Booking.AmountDue, request.Method);
        var paidAt = clock.GetUtcNow().UtcDateTime;

        var registered = await bookings.RegisterPaymentAsync(
            bookingId, new NewPayment(request.Method, charge.Adjustment, charge.Total, paidAt), ct);

        if (!registered)
        {
            notifications.AddConflict("booking", "Reserva nao esta mais disponivel para pagamento.");
            return null;
        }

        return new PaymentResponseDto(
            bookingId, request.Method, aggregate.Booking.AmountDue, charge.Adjustment, charge.Total, paidAt);
    }

    public async Task<CancellationResponseDto?> CancelAsync(int bookingId, CancellationToken ct)
    {
        var aggregate = await bookings.GetAggregateAsync(bookingId, ct);
        if (aggregate is null)
        {
            notifications.AddNotFound("booking", "Reserva nao encontrada.");
            return null;
        }

        if (aggregate.Booking.Status == BookingStatus.Cancelled)
        {
            notifications.AddConflict("booking", "Reserva ja esta cancelada.");
            return null;
        }

        var nowUtc = clock.GetUtcNow().UtcDateTime;
        var daysUntilDeparture = (aggregate.Flight.DepartureUtc - nowUtc).TotalDays;
        var paid = aggregate.Booking.Status == BookingStatus.Paid && aggregate.Payment is not null;
        var amountPaid = aggregate.Payment?.AmountPaid ?? 0m;
        double? hoursSincePayment = aggregate.Payment is null
            ? null
            : (nowUtc - aggregate.Payment.PaidAt).TotalHours;

        var refund = cancellationPolicy.CalculateRefund(
            aggregate.Booking.FareClassId, daysUntilDeparture, paid, amountPaid, hoursSincePayment);

        var cancelled = await bookings.CancelAsync(
            aggregate.Booking, refund.RefundPercentage, refund.RefundAmount, nowUtc, ct);

        if (!cancelled)
        {
            notifications.AddConflict("booking", "Reserva nao pode ser cancelada.");
            return null;
        }

        return new CancellationResponseDto(bookingId, refund.RefundPercentage, refund.RefundAmount, nowUtc);
    }

    private async Task<bool> ValidateAsync<T>(IValidator<T> validator, T instance, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid)
            return true;

        foreach (var error in result.Errors)
            notifications.AddNotification(error.PropertyName, error.ErrorMessage);
        return false;
    }

    private static BookingResponseDto ToResponse(BookingAggregate aggregate)
    {
        var booking = aggregate.Booking;

        var breakdown = new PriceBreakdownDto(
            booking.FarePrice, booking.PassengerCount, booking.Subtotal,
            booking.Taxes, booking.ServiceFee, booking.AmountDue);

        var passengers = aggregate.Passengers
            .Select(p => new PassengerDto(p.Name, p.Document))
            .ToList();

        var payment = aggregate.Payment is null
            ? null
            : new PaymentSummaryDto(
                aggregate.Payment.Method, aggregate.Payment.Adjustment,
                aggregate.Payment.AmountPaid, aggregate.Payment.PaidAt);

        return new BookingResponseDto(
            booking.Id, booking.CustomerId, booking.FlightId, booking.FareClassId, booking.Status,
            booking.PassengerCount, passengers, breakdown, payment, booking.CreatedAt);
    }
}
