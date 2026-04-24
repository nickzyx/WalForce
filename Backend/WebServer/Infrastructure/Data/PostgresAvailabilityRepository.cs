using Npgsql;
using NpgsqlTypes;
using WebServer.Domain.Models;
using WebServer.Domain.Repositories;

namespace WebServer.Infrastructure.Data;

public sealed class PostgresAvailabilityRepository(NpgsqlDataSource dataSource) : IAvailabilityRepository
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

    public async Task<AvailabilityRecord?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (!PostgresEmployeeIdMapper.TryGetEmployeeId(userId, out var employeeId))
        {
            return null;
        }

        await using var command = dataSource.CreateCommand(
            """
            SELECT date, timestart, timeend
            FROM public.availability
            WHERE employee_id = $1
            ORDER BY date, timestart;
            """);
        command.Parameters.AddWithValue(employeeId);

        var days = OrderedDays
            .ToDictionary(
                day => day,
                day => new AvailabilityDayRecord { Day = day, Windows = [] },
                StringComparer.OrdinalIgnoreCase);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var found = false;
        while (await reader.ReadAsync(cancellationToken))
        {
            found = true;
            var date = DateOnly.FromDateTime(reader.GetDateTime(0));
            var dayName = date.DayOfWeek.ToString();
            if (!days.TryGetValue(dayName, out var day))
            {
                continue;
            }

            day.Windows.Add(new AvailabilityWindowRecord
            {
                StartTime = TimeOnly.FromTimeSpan(reader.GetFieldValue<DateTimeOffset>(1).TimeOfDay),
                EndTime = TimeOnly.FromTimeSpan(reader.GetFieldValue<DateTimeOffset>(2).TimeOfDay)
            });
        }

        return found
            ? new AvailabilityRecord { UserId = userId, Days = OrderedDays.Select(day => days[day]).ToList() }
            : null;
    }

    public async Task<AvailabilityRecord> UpsertAsync(AvailabilityRecord availability, CancellationToken cancellationToken)
    {
        if (!PostgresEmployeeIdMapper.TryGetEmployeeId(availability.UserId, out var employeeId))
        {
            return availability;
        }

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var delete = new NpgsqlCommand("DELETE FROM public.availability WHERE employee_id = $1;", connection, transaction))
        {
            delete.Parameters.AddWithValue(employeeId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        var weekStart = GetCurrentWeekMonday();
        var entryIndex = 0;
        foreach (var day in availability.Days)
        {
            var dayOffset = Array.FindIndex(OrderedDays, item => string.Equals(item, day.Day, StringComparison.OrdinalIgnoreCase));
            if (dayOffset < 0)
            {
                continue;
            }

            var date = weekStart.AddDays(dayOffset);
            foreach (var window in day.Windows)
            {
                await using var insert = new NpgsqlCommand(
                    """
                    INSERT INTO public.availability (entry_id, employee_id, date, timestart, timeend)
                    VALUES ($1, $2, $3, $4, $5);
                    """,
                    connection,
                    transaction);
                insert.Parameters.AddWithValue($"{employeeId}-{++entryIndex}");
                insert.Parameters.AddWithValue(employeeId);
                insert.Parameters.AddWithValue(date);
                insert.Parameters.AddWithValue(NpgsqlDbType.TimeTz, new DateTimeOffset(DateTime.Today.Add(window.StartTime.ToTimeSpan())));
                insert.Parameters.AddWithValue(NpgsqlDbType.TimeTz, new DateTimeOffset(DateTime.Today.Add(window.EndTime.ToTimeSpan())));
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return availability;
    }

    private static DateOnly GetCurrentWeekMonday()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var offset = today.DayOfWeek == DayOfWeek.Sunday ? -6 : DayOfWeek.Monday - today.DayOfWeek;
        return today.AddDays(offset);
    }
}
