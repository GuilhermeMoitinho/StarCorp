using System.Net;
using System.Net.Http.Json;
using StarCorp.Business.Dtos;
using StarCorp.Business.Enums;

namespace StarCorp.IntegrationTests;

[Collection("api")]
public class BookingsEndpointTests(ApiFactory factory)
{
    private readonly ApiFactory _factory = factory;

    private static CreateBookingRequest BookingFor(int customerId, int flightId, int passengers = 1) =>
        new(customerId, flightId, FareClass.Economica,
            Enumerable.Range(1, passengers).Select(i => new PassengerDto($"Passageiro {i}", $"DOC{i}")).ToList());

    [Fact]
    public async Task Full_Flow_Create_Pay_Cancel_Should_Compute_Prices_And_Refund()
    {
        var client = _factory.CreateClient();
        var customerId = await TestData.InsertCustomerAsync(_factory.ConnectionString);
        var flightId = await TestData.InsertFlightAsync(_factory.ConnectionString, basePrice: 1000m);

        var create = await client.PostAsJsonAsync("/api/bookings", BookingFor(customerId, flightId), TestJson.Options);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var booking = await create.Content.ReadFromJsonAsync<BookingResponseDto>(TestJson.Options);
        Assert.NotNull(booking);
        Assert.Equal(BookingStatus.Pending, booking!.Status);
        Assert.Equal(1181.25m, booking.Breakdown.AmountDue);

        var pay = await client.PostAsJsonAsync($"/api/bookings/{booking.Id}/payment", new PaymentRequest(PaymentMethod.Pix), TestJson.Options);
        Assert.Equal(HttpStatusCode.OK, pay.StatusCode);
        var payment = await pay.Content.ReadFromJsonAsync<PaymentResponseDto>(TestJson.Options);
        Assert.Equal(-59.06m, payment!.Adjustment);
        Assert.Equal(1122.19m, payment.AmountPaid);

        var get = await client.GetFromJsonAsync<BookingResponseDto>($"/api/bookings/{booking.Id}", TestJson.Options);
        Assert.Equal(BookingStatus.Paid, get!.Status);
        Assert.NotNull(get.Payment);

        var cancel = await client.PostAsync($"/api/bookings/{booking.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);
        var refund = await cancel.Content.ReadFromJsonAsync<CancellationResponseDto>(TestJson.Options);
        Assert.Equal(100m, refund!.RefundPercentage);
        Assert.Equal(1122.19m, refund.RefundAmount);
    }

    [Fact]
    public async Task Create_Should_Return_422_For_Inactive_Customer()
    {
        var client = _factory.CreateClient();
        var customerId = await TestData.InsertCustomerAsync(_factory.ConnectionString, active: false);
        var flightId = await TestData.InsertFlightAsync(_factory.ConnectionString, basePrice: 500m);

        var response = await client.PostAsJsonAsync("/api/bookings", BookingFor(customerId, flightId), TestJson.Options);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return_409_When_No_Seats_Available()
    {
        var client = _factory.CreateClient();
        var customerId = await TestData.InsertCustomerAsync(_factory.ConnectionString);
        var flightId = await TestData.InsertFlightAsync(_factory.ConnectionString, basePrice: 500m, economySeats: 0);

        var response = await client.PostAsJsonAsync("/api/bookings", BookingFor(customerId, flightId), TestJson.Options);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return_400_When_No_Passengers()
    {
        var client = _factory.CreateClient();
        var request = new CreateBookingRequest(1, 1, FareClass.Economica, []);

        var response = await client.PostAsJsonAsync("/api/bookings", request, TestJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Should_Return_404_For_Unknown_Booking()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/bookings/999999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Pay_Twice_Should_Return_409()
    {
        var client = _factory.CreateClient();
        var customerId = await TestData.InsertCustomerAsync(_factory.ConnectionString);
        var flightId = await TestData.InsertFlightAsync(_factory.ConnectionString, basePrice: 500m);

        var create = await client.PostAsJsonAsync("/api/bookings", BookingFor(customerId, flightId), TestJson.Options);
        var booking = await create.Content.ReadFromJsonAsync<BookingResponseDto>(TestJson.Options);

        var firstPayment = await client.PostAsJsonAsync($"/api/bookings/{booking!.Id}/payment", new PaymentRequest(PaymentMethod.Boleto), TestJson.Options);
        Assert.Equal(HttpStatusCode.OK, firstPayment.StatusCode);

        var secondPayment = await client.PostAsJsonAsync($"/api/bookings/{booking.Id}/payment", new PaymentRequest(PaymentMethod.Boleto), TestJson.Options);
        Assert.Equal(HttpStatusCode.Conflict, secondPayment.StatusCode);
    }
}
