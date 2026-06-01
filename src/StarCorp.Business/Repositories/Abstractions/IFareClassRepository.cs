using StarCorp.Business.Entities;
using StarCorp.Business.Enums;

namespace StarCorp.Business.Repositories.Abstractions;

public interface IFareClassRepository
{
    Task<FareClassInfo?> GetByIdAsync(FareClass id, CancellationToken ct);
}
