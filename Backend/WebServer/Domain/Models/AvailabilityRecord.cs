namespace WebServer.Domain.Models;

public sealed class AvailabilityRecord
{
    public Guid UserId { get; init; }

    public string? Notes { get; init; }

    public List<AvailabilityDayRecord> Days { get; init; } = [];
}
