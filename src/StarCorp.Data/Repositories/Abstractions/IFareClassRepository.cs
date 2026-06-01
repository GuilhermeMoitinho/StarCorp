using StarCorp.Data.Entities;
using StarCorp.Data.Enums;

namespace StarCorp.Data.Repositories.Abstractions;

public interface IFareClassRepository
{
    Task<FareClassInfo?> GetByIdAsync(FareClass id, CancellationToken ct);
}
