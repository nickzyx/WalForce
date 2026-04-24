using Npgsql;
using WebServer.Domain;
using WebServer.Domain.Models;
using WebServer.Domain.Repositories;

namespace WebServer.Infrastructure.Data;

public sealed class PostgresUserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    public async Task<UserRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            """
            SELECT e.employeeid, e.f_name, e.l_name, e.role, l.email, l.password
            FROM public.logins l
            INNER JOIN public.employees e ON e.employeeid = l.employee_id
            WHERE lower(l.email) = lower($1)
            LIMIT 1;
            """);
        command.Parameters.AddWithValue(email);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? ReadUser(reader)
            : null;
    }

    public async Task<UserRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!PostgresEmployeeIdMapper.TryGetEmployeeId(id, out var employeeId))
        {
            return null;
        }

        await using var command = dataSource.CreateCommand(
            """
            SELECT e.employeeid, e.f_name, e.l_name, e.role, l.email, l.password
            FROM public.employees e
            LEFT JOIN public.logins l ON l.employee_id = e.employeeid
            WHERE e.employeeid = $1
            LIMIT 1;
            """);
        command.Parameters.AddWithValue(employeeId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? ReadUser(reader)
            : null;
    }

    public async Task<IReadOnlyList<UserRecord>> GetEmployeesAsync(CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            """
            SELECT e.employeeid, e.f_name, e.l_name, e.role, l.email, l.password
            FROM public.employees e
            LEFT JOIN public.logins l ON l.employee_id = e.employeeid
            WHERE lower(e.role) = 'employee'
            ORDER BY e.l_name, e.f_name;
            """);

        var users = new List<UserRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(ReadUser(reader));
        }

        return users;
    }

    private static UserRecord ReadUser(NpgsqlDataReader reader)
    {
        var role = reader.GetString(3);
        var normalizedRole = string.Equals(role, Roles.Manager, StringComparison.OrdinalIgnoreCase)
            ? Roles.Manager
            : Roles.Employee;

        return new UserRecord
        {
            Id = PostgresEmployeeIdMapper.ToUserId(reader.GetInt32(0)),
            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Role = normalizedRole,
            Title = normalizedRole == Roles.Manager ? "Store Manager" : "Employee",
            Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            PasswordHash = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
        };
    }
}
