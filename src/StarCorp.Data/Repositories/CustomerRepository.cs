using Dapper;
using StarCorp.Data.Connection;
using StarCorp.Data.Entities;
using StarCorp.Data.Repositories.Abstractions;

namespace StarCorp.Data.Repositories;

public sealed class CustomerRepository(IDbConnectionFactory factory) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct)
    {
        await using var conn = factory.Create();
        var command = new CommandDefinition(
            "SELECT Id, Name, Document, Email, IsActive, CreatedAt FROM Customers WHERE Id = @id;",
            new { id }, cancellationToken: ct);
        return await conn.QuerySingleOrDefaultAsync<Customer>(command);
    }
}
