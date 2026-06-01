using StarCorp.Business.Enums;
using StarCorp.Business.Pricing;

namespace StarCorp.Business.Dtos;

public record CreateBookingRequest(
    int CustomerId,
    int FlightId,
    FareClass FareClass,
    IReadOnlyList<PassengerDto> Passengers);

public record PassengerDto(string Name, string Document);

public record PaymentSummaryDto(
    PaymentMethod Method,
    decimal Adjustment,
    decimal AmountPaid,
    DateTime PaidAt);

public record BookingResponseDto(
    int Id,
    int CustomerId,
    int FlightId,
    FareClass FareClass,
    BookingStatus Status,
    int PassengerCount,
    IReadOnlyList<PassengerDto> Passengers,
    PriceBreakdown Breakdown,
    PaymentSummaryDto? Payment,
    DateTime CreatedAt);
