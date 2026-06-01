using Dapper;
using Microsoft.Data.SqlClient;

namespace StarCorp.IntegrationTests;

// Insere dados direto no banco para montar cenarios de teste deterministicos.
internal static class TestData
{
    public static async Task<int> InsertCustomerAsync(string connectionString, bool active = true)
    {
        await using var conn = new SqlConnection(connectionString);
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Customers (Name, Document, Email, IsActive)
              VALUES (@Name, @Document, @Email, @Active);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            new
            {
                Name = "Cliente Teste",
                Document = Guid.NewGuid().ToString("N")[..11],
                Email = "teste@exemplo.com",
                Active = active
            });
    }

    public static async Task<int> InsertFlightAsync(
        string connectionString,
        decimal basePrice,
        int economySeats = 5,
        int businessSeats = 2,
        string origin = "Origem",
        string destination = "Destino",
        DateTime? departureUtc = null)
    {
        var departure = departureUtc ?? DateTime.UtcNow.AddDays(30);
        await using var conn = new SqlConnection(connectionString);

        var airlineId = await conn.ExecuteScalarAsync<int>(
            @"IF NOT EXISTS (SELECT 1 FROM Airlines WHERE IataCode = 'TT')
                  INSERT INTO Airlines (Name, IataCode) VALUES ('Test Air', 'TT');
              SELECT Id FROM Airlines WHERE IataCode = 'TT';");

        var flightId = await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Flights (AirlineId, OriginCity, DestinationCity, DepartureUtc, ArrivalUtc, BasePrice)
              VALUES (@AirlineId, @Origin, @Destination, @Departure, @Arrival, @BasePrice);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            new
            {
                AirlineId = airlineId,
                Origin = origin,
                Destination = destination,
                Departure = departure,
                Arrival = departure.AddHours(2),
                BasePrice = basePrice
            });

        await conn.ExecuteAsync(
            @"INSERT INTO FlightSeats (FlightId, FareClassId, SeatsAvailable)
              VALUES (@FlightId, 1, @Eco), (@FlightId, 2, @Exec);",
            new { FlightId = flightId, Eco = economySeats, Exec = businessSeats });

        return flightId;
    }
}
