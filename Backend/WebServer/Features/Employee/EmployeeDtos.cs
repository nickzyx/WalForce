using WebServer.Domain.Models;

namespace WebServer.Features.Employee;

public sealed record ProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string Title);

public sealed record ScheduleShiftDto(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string RoleLabel,
    string? Note);

public sealed record WeeklyAvailabilityDto(
    string? Notes,
    IReadOnlyList<AvailabilityDayDto> Days);

public sealed record AvailabilityDayDto(
    string Day,
    IReadOnlyList<AvailabilityWindowDto> Windows);

public sealed record AvailabilityWindowDto(
    TimeOnly StartTime,
    TimeOnly EndTime);

public sealed record UpdateAvailabilityRequest(
    string? Notes,
    IReadOnlyList<AvailabilityDayRequest>? Days);

public sealed record AvailabilityDayRequest(
    string Day,
    IReadOnlyList<AvailabilityWindowRequest>? Windows);

public sealed record AvailabilityWindowRequest(
    TimeOnly StartTime,
    TimeOnly EndTime);

internal static class AvailabilityMappings
{
    private static readonly string[] OrderedDays =
    [
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
        "Sunday"
    ];

    public static WeeklyAvailabilityDto ToDto(this AvailabilityRecord availability)
        => new(
            availability.Notes,
            OrderedDays
                .Select(day =>
                {
                    var match = availability.Days.FirstOrDefault(item => string.Equals(item.Day, day, StringComparison.OrdinalIgnoreCase));
                    var windows = (match?.Windows ?? [])
                        .Select(window => new AvailabilityWindowDto(window.StartTime, window.EndTime))
                        .ToArray();

                    return new AvailabilityDayDto(day, windows);
                })
                .ToArray());

    public static AvailabilityRecord ToRecord(Guid userId, UpdateAvailabilityRequest request)
    {
        var dayLookup = (request.Days ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item.Day))
            .GroupBy(item => NormalizeDay(item.Day))
            .Where(group => group.Key is not null)
            .ToDictionary(
                group => group.Key!,
                group => group.Last(),
                StringComparer.OrdinalIgnoreCase);

        var days = OrderedDays
            .Select(day =>
            {
                var windows = dayLookup.TryGetValue(day, out var requestDay)
                    ? (requestDay.Windows ?? [])
                        .Select(window => new AvailabilityWindowRecord
                        {
                            StartTime = window.StartTime,
                            EndTime = window.EndTime
                        })
                        .ToList()
                    : [];

                return new AvailabilityDayRecord
                {
                    Day = day,
                    Windows = windows
                };
            })
            .ToList();

        return new AvailabilityRecord
        {
            UserId = userId,
            Notes = request.Notes,
            Days = days
        };
    }

    public static AvailabilityRecord Empty(Guid userId)
        => new()
        {
            UserId = userId,
            Days = OrderedDays
                .Select(day => new AvailabilityDayRecord
                {
                    Day = day,
                    Windows = []
                })
                .ToList()
        };

    private static string? NormalizeDay(string day)
        => OrderedDays.FirstOrDefault(value => string.Equals(value, day, StringComparison.OrdinalIgnoreCase));
}
