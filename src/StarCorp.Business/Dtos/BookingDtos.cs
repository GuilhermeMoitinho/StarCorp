using StarCorp.Data.Enums;

namespace StarCorp.Business.Dtos;

public record CreateBookingRequest(
    int CustomerId,
    int FlightId,
    FareClass FareClass,
    IReadOnlyList<PassengerDto> Passengers);

public record PassengerDto(string Name, string Document);

public record PriceBreakdownDto(
    decimal FarePricePerPassenger,
    int Passengers,
    decimal Subtotal,
    decimal Taxes,
    decimal ServiceFee,
    decimal AmountDue);

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
    PriceBreakdownDto Breakdown,
    PaymentSummaryDto? Payment,
    DateTime CreatedAt);
