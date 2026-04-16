namespace WebServer.Domain.Models;

public sealed class AvailabilityWindowRecord
{
    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}
