namespace WebServer.Domain.Models;

public sealed class ShiftRecord
{
    public Guid Id { get; init; }

    public Guid EmployeeId { get; init; }

    public DateOnly Date { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }

    public string RoleLabel { get; init; } = string.Empty;

    public string? Note { get; init; }
}
