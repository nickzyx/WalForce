namespace WebServer.Domain.Models;

public sealed class AvailabilityDayRecord
{
    public string Day { get; init; } = string.Empty;

    public List<AvailabilityWindowRecord> Windows { get; init; } = [];
}
