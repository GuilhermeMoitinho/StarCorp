using StarCorp.Business.Entities;

namespace StarCorp.Business.Repositories.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct);
}
