using Microsoft.AspNetCore.Http.HttpResults;
using WebServer.Domain;
using WebServer.Domain.Repositories;

namespace WebServer.Features.Manager;

public static class ManagerEndpoints
{
    public static IEndpointRouteBuilder MapManagerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/manager")
            .WithTags("Manager")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Manager));

        group.MapGet("/roster", GetRosterAsync)
            .WithName("GetManagerRoster")
            .WithSummary("Get the full employee roster for the prototype team.");

        group.MapGet("/schedule", GetScheduleAsync)
            .WithName("GetManagerSchedule")
            .WithSummary("Get the assigned team schedule for a date range.");

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<ManagerRosterItemDto>>> GetRosterAsync(
        IUserRepository users,
        CancellationToken cancellationToken)
    {
        var roster = await users.GetEmployeesAsync(cancellationToken);
        var response = roster
            .Select(user => new ManagerRosterItemDto(user.Id, user.FirstName, user.LastName, user.Email, user.Title))
            .ToArray();

        return TypedResults.Ok<IReadOnlyList<ManagerRosterItemDto>>(response);
    }

    private static async Task<Results<Ok<IReadOnlyList<ManagerScheduleShiftDto>>, ValidationProblem>> GetScheduleAsync(
        DateOnly from,
        DateOnly to,
        IUserRepository users,
        IScheduleRepository schedules,
        CancellationToken cancellationToken)
    {
        if (to < from)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["to"] = ["The 'to' date must be on or after the 'from' date."]
            });
        }

        var employeeLookup = (await users.GetEmployeesAsync(cancellationToken))
            .ToDictionary(user => user.Id);

        var shifts = await schedules.GetTeamScheduleAsync(from, to, cancellationToken);
        var response = shifts
            .Where(shift => employeeLookup.ContainsKey(shift.EmployeeId))
            .Select(shift =>
            {
                var employee = employeeLookup[shift.EmployeeId];
                return new ManagerScheduleShiftDto(
                    shift.Id,
                    shift.EmployeeId,
                    $"{employee.FirstName} {employee.LastName}",
                    shift.Date,
                    shift.StartTime,
                    shift.EndTime,
                    shift.RoleLabel,
                    shift.Note);
            })
            .ToArray();

        return TypedResults.Ok<IReadOnlyList<ManagerScheduleShiftDto>>(response);
    }
}
