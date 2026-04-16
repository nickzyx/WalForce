namespace WebServer.Features.Manager;

public sealed record ManagerRosterItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Title);

public sealed record ManagerScheduleShiftDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string RoleLabel,
    string? Note);
