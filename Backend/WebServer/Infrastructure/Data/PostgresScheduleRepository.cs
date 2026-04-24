using Npgsql;
using WebServer.Domain.Models;
using WebServer.Domain.Repositories;

namespace WebServer.Infrastructure.Data;

public sealed class PostgresScheduleRepository(NpgsqlDataSource dataSource) : IScheduleRepository
{
    public Task<IReadOnlyList<ShiftRecord>> GetEmployeeScheduleAsync(
        Guid employeeId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        if (!PostgresEmployeeIdMapper.TryGetEmployeeId(employeeId, out var databaseEmployeeId))
        {
            return Task.FromResult<IReadOnlyList<ShiftRecord>>([]);
        }

        return GetScheduleAsync(
            """
            SELECT shift_id, employee_id, date, timestart, timeend, role_label, note
            FROM public.shifts
            WHERE employee_id = $1 AND date >= $2 AND date <= $3
            ORDER BY date, timestart;
            """,
            command =>
            {
                command.Parameters.AddWithValue(databaseEmployeeId);
                command.Parameters.AddWithValue(from);
                command.Parameters.AddWithValue(to);
            },
            cancellationToken);
    }

    public Task<IReadOnlyList<ShiftRecord>> GetTeamScheduleAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
        => GetScheduleAsync(
            """
            SELECT shift_id, employee_id, date, timestart, timeend, role_label, note
            FROM public.shifts
            WHERE date >= $1 AND date <= $2
            ORDER BY date, timestart;
            """,
            command =>
            {
                command.Parameters.AddWithValue(from);
                command.Parameters.AddWithValue(to);
            },
            cancellationToken);

    private async Task<IReadOnlyList<ShiftRecord>> GetScheduleAsync(
        string sql,
        Action<NpgsqlCommand> bindParameters,
        CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(sql);
        bindParameters(command);

        var shifts = new List<ShiftRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            shifts.Add(new ShiftRecord
            {
                Id = reader.GetGuid(0),
                EmployeeId = PostgresEmployeeIdMapper.ToUserId(reader.GetInt32(1)),
                Date = reader.GetFieldValue<DateOnly>(2),
                StartTime = reader.GetFieldValue<TimeOnly>(3),
                EndTime = reader.GetFieldValue<TimeOnly>(4),
                RoleLabel = reader.GetString(5),
                Note = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        }

        return shifts;
    }
}
