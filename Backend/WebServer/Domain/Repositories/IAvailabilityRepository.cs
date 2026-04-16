using WebServer.Domain.Models;

namespace WebServer.Domain.Repositories;

public interface IAvailabilityRepository
{
    Task<AvailabilityRecord?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<AvailabilityRecord> UpsertAsync(AvailabilityRecord availability, CancellationToken cancellationToken);
}
