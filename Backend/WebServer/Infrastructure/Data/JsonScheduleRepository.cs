using WebServer.Domain.Models;
using WebServer.Domain.Repositories;

namespace WebServer.Infrastructure.Data;

public sealed class JsonScheduleRepository(JsonFileDataStore dataStore) : IScheduleRepository
{
    private const string SchedulesFileName = "schedules.json";

    public async Task<IReadOnlyList<ShiftRecord>> GetEmployeeScheduleAsync(
        Guid employeeId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var shifts = await dataStore.ReadListAsync<ShiftRecord>(SchedulesFileName, cancellationToken);
        return FilterAndOrder(shifts.Where(shift => shift.EmployeeId == employeeId), from, to);
    }

    public async Task<IReadOnlyList<ShiftRecord>> GetTeamScheduleAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var shifts = await dataStore.ReadListAsync<ShiftRecord>(SchedulesFileName, cancellationToken);
        return FilterAndOrder(shifts, from, to);
    }

    private static ShiftRecord[] FilterAndOrder(IEnumerable<ShiftRecord> shifts, DateOnly from, DateOnly to)
        => shifts
            .Where(shift => shift.Date >= from && shift.Date <= to)
            .OrderBy(shift => shift.Date)
            .ThenBy(shift => shift.StartTime)
            .ToArray();
}
