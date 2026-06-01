using Dapper;
using StarCorp.Data.Connection;
using StarCorp.Business.Entities;
using StarCorp.Business.Enums;
using StarCorp.Business.Queries;
using StarCorp.Business.Repositories;
using StarCorp.Business.Repositories.Abstractions;

namespace StarCorp.Data.Repositories;

public sealed class BookingRepository(IDbConnectionFactory factory) : IBookingRepository
{
    public async Task<int?> CreateAsync(NewBooking booking, IReadOnlyList<NewPassenger> passengers, CancellationToken ct)
    {
        await using var conn = factory.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var seatsTaken = await conn.ExecuteAsync(new CommandDefinition(
            @"UPDATE FlightSeats
              SET SeatsAvailable = SeatsAvailable - @Count
              WHERE FlightId = @FlightId AND FareClassId = @FareClassId AND SeatsAvailable >= @Count;",
            new { Count = booking.PassengerCount, booking.FlightId, FareClassId = (byte)booking.FareClassId },
            tx, cancellationToken: ct));

        if (seatsTaken == 0)
        {
            await tx.RollbackAsync(ct);
            return null;
        }

        var bookingId = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            @"INSERT INTO Bookings
                (CustomerId, FlightId, FareClassId, Status, PassengerCount, FarePrice, Subtotal, Taxes, ServiceFee, AmountDue, CreatedAt)
              VALUES
                (@CustomerId, @FlightId, @FareClassId, @Status, @PassengerCount, @FarePrice, @Subtotal, @Taxes, @ServiceFee, @AmountDue, @CreatedAt);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            new
            {
                booking.CustomerId,
                booking.FlightId,
                FareClassId = (byte)booking.FareClassId,
                Status = (byte)BookingStatus.Pending,
                booking.PassengerCount,
                booking.FarePrice,
                booking.Subtotal,
                booking.Taxes,
                booking.ServiceFee,
                booking.AmountDue,
                CreatedAt = booking.CreatedAtUtc
            },
            tx, cancellationToken: ct));

        if (passengers.Count > 0)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                "INSERT INTO BookingPassengers (BookingId, Name, Document) VALUES (@BookingId, @Name, @Document);",
                passengers.Select(p => new { BookingId = bookingId, p.Name, p.Document }),
                tx, cancellationToken: ct));
        }

        await tx.CommitAsync(ct);
        return bookingId;
    }

    public async Task<BookingAggregate?> GetAggregateAsync(int id, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, CustomerId, FlightId, FareClassId, Status, PassengerCount, FarePrice, Subtotal, Taxes, ServiceFee, AmountDue, CreatedAt
FROM Bookings WHERE Id = @id;

SELECT f.Id, f.AirlineId, f.OriginCity, f.DestinationCity, f.DepartureUtc, f.ArrivalUtc, f.BasePrice
FROM Flights f JOIN Bookings b ON b.FlightId = f.Id WHERE b.Id = @id;

SELECT Id, BookingId, Name, Document FROM BookingPassengers WHERE BookingId = @id ORDER BY Id;

SELECT Id, BookingId, Method, Adjustment, AmountPaid, PaidAt FROM Payments WHERE BookingId = @id;

SELECT Id, BookingId, RefundPercentage, RefundAmount, CancelledAt FROM Cancellations WHERE BookingId = @id;";

        await using var conn = factory.Create();
        await conn.OpenAsync(ct);
        using var multi = await conn.QueryMultipleAsync(new CommandDefinition(sql, new { id }, cancellationToken: ct));

        var booking = await multi.ReadSingleOrDefaultAsync<Booking>();
        if (booking is null)
            return null;

        var flight = await multi.ReadSingleAsync<Flight>();
        var passengers = (await multi.ReadAsync<BookingPassenger>()).ToList();
        var payment = await multi.ReadSingleOrDefaultAsync<Payment>();
        var cancellation = await multi.ReadSingleOrDefaultAsync<Cancellation>();

        return new BookingAggregate(booking, flight, passengers, payment, cancellation);
    }

    public async Task<bool> RegisterPaymentAsync(int bookingId, NewPayment payment, CancellationToken ct)
    {
        await using var conn = factory.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var updated = await conn.ExecuteAsync(new CommandDefinition(
            "UPDATE Bookings SET Status = @Paid WHERE Id = @Id AND Status = @Pending;",
            new { Paid = (byte)BookingStatus.Paid, Id = bookingId, Pending = (byte)BookingStatus.Pending },
            tx, cancellationToken: ct));

        if (updated == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        await conn.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO Payments (BookingId, Method, Adjustment, AmountPaid, PaidAt)
              VALUES (@BookingId, @Method, @Adjustment, @AmountPaid, @PaidAt);",
            new
            {
                BookingId = bookingId,
                Method = (byte)payment.Method,
                payment.Adjustment,
                payment.AmountPaid,
                PaidAt = payment.PaidAtUtc
            },
            tx, cancellationToken: ct));

        await tx.CommitAsync(ct);
        return true;
    }

    public async Task<bool> CancelAsync(Booking booking, decimal refundPercentage, decimal refundAmount, DateTime cancelledAtUtc, CancellationToken ct)
    {
        await using var conn = factory.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var updated = await conn.ExecuteAsync(new CommandDefinition(
            "UPDATE Bookings SET Status = @Cancelled WHERE Id = @Id AND Status IN (@Pending, @Paid);",
            new
            {
                Cancelled = (byte)BookingStatus.Cancelled,
                Id = booking.Id,
                Pending = (byte)BookingStatus.Pending,
                Paid = (byte)BookingStatus.Paid
            },
            tx, cancellationToken: ct));

        if (updated == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        await conn.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO Cancellations (BookingId, RefundPercentage, RefundAmount, CancelledAt)
              VALUES (@BookingId, @RefundPercentage, @RefundAmount, @CancelledAt);",
            new { BookingId = booking.Id, RefundPercentage = refundPercentage, RefundAmount = refundAmount, CancelledAt = cancelledAtUtc },
            tx, cancellationToken: ct));

        await conn.ExecuteAsync(new CommandDefinition(
            @"UPDATE FlightSeats SET SeatsAvailable = SeatsAvailable + @Count
              WHERE FlightId = @FlightId AND FareClassId = @FareClassId;",
            new { Count = booking.PassengerCount, booking.FlightId, FareClassId = (byte)booking.FareClassId },
            tx, cancellationToken: ct));

        await tx.CommitAsync(ct);
        return true;
    }
}
