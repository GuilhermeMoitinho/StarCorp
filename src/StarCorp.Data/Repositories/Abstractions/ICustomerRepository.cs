using StarCorp.Data.Entities;

namespace StarCorp.Data.Repositories.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct);
}
