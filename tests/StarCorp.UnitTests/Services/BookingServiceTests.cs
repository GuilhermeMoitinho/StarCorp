using NSubstitute;
using StarCorp.Business.Dtos;
using StarCorp.Business.Notifications;
using StarCorp.Business.Notifications.Abstractions;
using StarCorp.Business.Pricing;
using StarCorp.Business.Services;
using StarCorp.Business.Validators;
using StarCorp.Data.Entities;
using StarCorp.Data.Enums;
using StarCorp.Data.Queries;
using StarCorp.Data.Repositories;
using StarCorp.Data.Repositories.Abstractions;

namespace StarCorp.UnitTests.Services;

public class BookingServiceTests
{
    private readonly IBookingRepository _bookings = Substitute.For<IBookingRepository>();
    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IFlightRepository _flights = Substitute.For<IFlightRepository>();
    private readonly IFareClassRepository _fareClasses = Substitute.For<IFareClassRepository>();
    private readonly NotificationContext _notifications = new();
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _sut = new BookingService(
            _bookings, _customers, _flights, _fareClasses,
            new PricingCalculator(), new CancellationPolicy(),
            _notifications,
            new CreateBookingValidator(), new PaymentValidator(),
            TimeProvider.System);
    }

    private static CreateBookingRequest ValidRequest() =>
        new(CustomerId: 1, FlightId: 1, FareClass: FareClass.Economica, Passengers: [new PassengerDto("Ana", "111")]);

    private static Customer ActiveCustomer() => new(1, "Ana", "111", "ana@exemplo.com", IsActive: true, DateTime.UtcNow);
    private static Flight SampleFlight() =>
        new(1, 1, "Origem", "Destino", DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(10).AddHours(1), 500m);

    [Fact]
    public async Task CreateAsync_Should_NotFound_When_Customer_Missing()
    {
        _customers.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Customer?)null);

        var result = await _sut.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(NotificationType.NotFound, _notifications.Type);
    }

    [Fact]
    public async Task CreateAsync_Should_Unprocessable_When_Customer_Inactive()
    {
        _customers.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(ActiveCustomer() with { IsActive = false });

        var result = await _sut.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(NotificationType.UnprocessableEntity, _notifications.Type);
    }

    [Fact]
    public async Task CreateAsync_Should_Conflict_When_No_Seats()
    {
        _customers.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ActiveCustomer());
        _flights.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(SampleFlight());
        _fareClasses.GetByIdAsync(FareClass.Economica, Arg.Any<CancellationToken>())
            .Returns(new FareClassInfo(FareClass.Economica, "Economica", 1.0m));
        _bookings.CreateAsync(Arg.Any<NewBooking>(), Arg.Any<IReadOnlyList<NewPassenger>>(), Arg.Any<CancellationToken>())
            .Returns((int?)null);

        var result = await _sut.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(NotificationType.Conflict, _notifications.Type);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_Breakdown_When_Valid()
    {
        _customers.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(ActiveCustomer());
        _flights.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(SampleFlight());
        _fareClasses.GetByIdAsync(FareClass.Economica, Arg.Any<CancellationToken>())
            .Returns(new FareClassInfo(FareClass.Economica, "Economica", 1.0m));
        _bookings.CreateAsync(Arg.Any<NewBooking>(), Arg.Any<IReadOnlyList<NewPassenger>>(), Arg.Any<CancellationToken>())
            .Returns(42);

        var booking = new Booking(42, 1, 1, FareClass.Economica, BookingStatus.Pending, 1, 500m, 500m, 85m, 29.25m, 614.25m, DateTime.UtcNow);
        _bookings.GetAggregateAsync(42, Arg.Any<CancellationToken>())
            .Returns(new BookingAggregate(booking, SampleFlight(), [new BookingPassenger(1, 42, "Ana", "111")], null, null));

        var result = await _sut.CreateAsync(ValidRequest(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(_notifications.HasNotifications);
        Assert.Equal(42, result!.Id);
        Assert.Equal(614.25m, result.Breakdown.AmountDue);
    }

    [Fact]
    public async Task PayAsync_Should_Conflict_When_Already_Paid()
    {
        var booking = new Booking(7, 1, 1, FareClass.Economica, BookingStatus.Paid, 1, 500m, 500m, 85m, 29.25m, 614.25m, DateTime.UtcNow);
        var payment = new Payment(1, 7, PaymentMethod.Pix, -30.71m, 583.54m, DateTime.UtcNow);
        _bookings.GetAggregateAsync(7, Arg.Any<CancellationToken>())
            .Returns(new BookingAggregate(booking, SampleFlight(), [], payment, null));

        var result = await _sut.PayAsync(7, new PaymentRequest(PaymentMethod.CreditCard), CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(NotificationType.Conflict, _notifications.Type);
    }
}
