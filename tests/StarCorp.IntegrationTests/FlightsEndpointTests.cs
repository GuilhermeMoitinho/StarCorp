using System.Net;
using System.Net.Http.Json;
using StarCorp.Business.Dtos;
using StarCorp.Data.Enums;
using StarCorp.Data.Pagination;

namespace StarCorp.IntegrationTests;

[Collection("api")]
public class FlightsEndpointTests(ApiFactory factory)
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task Search_Should_Return_Both_Fare_Classes_For_A_Flight()
    {
        var client = _factory.CreateClient();
        var origin = $"O{Guid.NewGuid():N}"[..20];
        await TestData.InsertFlightAsync(_factory.ConnectionString, basePrice: 400m, origin: origin);

        var response = await client.GetAsync($"/api/flights?originCity={Uri.EscapeDataString(origin)}&pageSize=50");

        response.EnsureSuccessStatusCode();
        var page = await response.Content.ReadFromJsonAsync<PagedResult<FlightOfferDto>>(TestJson.Options);
        Assert.NotNull(page);
        Assert.Equal(2, page!.TotalItems);
        Assert.All(page.Items, offer => Assert.Equal(origin, offer.OriginCity));
    }

    [Fact]
    public async Task Search_Should_Filter_By_FareClass_And_Apply_Multiplier()
    {
        var client = _factory.CreateClient();
        var origin = $"E{Guid.NewGuid():N}"[..20];
        await TestData.InsertFlightAsync(_factory.ConnectionString, basePrice: 1000m, origin: origin);

        var response = await client.GetAsync($"/api/flights?originCity={Uri.EscapeDataString(origin)}&fareClass=Executiva");

        response.EnsureSuccessStatusCode();
        var page = await response.Content.ReadFromJsonAsync<PagedResult<FlightOfferDto>>(TestJson.Options);
        var offer = Assert.Single(page!.Items);
        Assert.Equal(FareClass.Executiva, offer.FareClass);
        Assert.Equal(2500.00m, offer.Price); // 1000 * 2.5
    }

    [Fact]
    public async Task Search_Should_Return_400_When_Price_Range_Is_Invalid()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights?minPrice=500&maxPrice=100");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
