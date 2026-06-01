using Dapper;
using StarCorp.Data.Connection;
using StarCorp.Data.Entities;
using StarCorp.Data.Enums;
using StarCorp.Data.Repositories.Abstractions;

namespace StarCorp.Data.Repositories;

public sealed class FareClassRepository(IDbConnectionFactory factory) : IFareClassRepository
{
    public async Task<FareClassInfo?> GetByIdAsync(FareClass id, CancellationToken ct)
    {
        await using var conn = factory.Create();
        var command = new CommandDefinition(
            "SELECT Id, Name, PriceMultiplier FROM FareClasses WHERE Id = @id;",
            new { id = (byte)id }, cancellationToken: ct);
        return await conn.QuerySingleOrDefaultAsync<FareClassInfo>(command);
    }
}
