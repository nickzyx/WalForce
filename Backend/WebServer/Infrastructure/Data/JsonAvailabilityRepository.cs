using WebServer.Domain.Models;
using WebServer.Domain.Repositories;

namespace WebServer.Infrastructure.Data;

public sealed class JsonAvailabilityRepository(JsonFileDataStore dataStore) : IAvailabilityRepository
{
    private const string AvailabilityFileName = "availability.json";

    private static readonly SemaphoreSlim UpdateLock = new(1, 1);

    public async Task<AvailabilityRecord?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var records = await dataStore.ReadListAsync<AvailabilityRecord>(AvailabilityFileName, cancellationToken);
        return records.FirstOrDefault(record => record.UserId == userId);
    }

    public async Task<AvailabilityRecord> UpsertAsync(AvailabilityRecord availability, CancellationToken cancellationToken)
    {
        await UpdateLock.WaitAsync(cancellationToken);
        try
        {
            var records = await dataStore.ReadListAsync<AvailabilityRecord>(AvailabilityFileName, cancellationToken);
            var index = records.FindIndex(record => record.UserId == availability.UserId);

            if (index >= 0)
            {
                records[index] = availability;
            }
            else
            {
                records.Add(availability);
            }

            await dataStore.WriteAsync(AvailabilityFileName, records, cancellationToken);
            return availability;
        }
        finally
        {
            UpdateLock.Release();
        }
    }
}
